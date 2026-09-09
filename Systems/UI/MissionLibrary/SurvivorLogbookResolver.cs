using System;
using System.Collections;
using System.Reflection;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Puente de sólo lectura hacia el Logbook nativo de Risk of Rain 2.
    ///
    /// USU NO persiste un estado paralelo de lore. Para cada consulta se usan
    /// las categorías nativas de LogBookController y sus buildEntries para el
    /// UserProfile local. De ese modo la visibilidad de Notas sigue exactamente
    /// el progreso que conoce el Diario del juego.
    ///
    /// El resolver sólo decide si el lore puede revelarse. El texto mostrado
    /// continúa viniendo de SurvivorContentProfile.Lore; no se copian stats ni
    /// otros campos visuales del Logbook.
    /// </summary>
    internal static class SurvivorLogbookResolver
    {
        internal enum NativeEntryState
        {
            Unknown,
            Missing,
            Locked,
            Available
        }

        internal sealed class Result
        {
            public NativeEntryState State = NativeEntryState.Unknown;
            public string StatusName = "";

            public bool HasNativeEntry
            {
                get
                {
                    return State != NativeEntryState.Missing &&
                           State != NativeEntryState.Unknown;
                }
            }

            public bool CanRevealLore
            {
                get
                {
                    return State == NativeEntryState.Available;
                }
            }
        }


        public static Result Resolve(
            SurvivorContentProfile profile
        )
        {
            Result result = new Result();

            if (
                profile == null ||
                profile.SurvivorDef == null
            )
            {
                return result;
            }

            UserProfile userProfile = TryGetLocalProfile();

            if (userProfile == null)
            {
                return result;
            }

            Type controllerType =
                FindType("RoR2.UI.LogBook.LogBookController");

            if (controllerType == null)
            {
                return result;
            }

            bool categoriesResolved = false;
            bool builtAnyEntries = false;
            object matchingEntry = null;

            // Camino principal: usar las mismas Category/buildEntries que usa
            // el Logbook para construir sus páginas para el UserProfile local.
            object categories =
                ReadStaticMember(
                    controllerType,
                    "categories"
                );

            if (categories is IEnumerable categoryEnumerable)
            {
                categoriesResolved = true;

                foreach (object category in categoryEnumerable)
                {
                    if (category == null)
                    {
                        continue;
                    }

                    object entries =
                        TryBuildCategoryEntries(
                            category,
                            userProfile
                        );

                    if (!(entries is IEnumerable entryEnumerable))
                    {
                        continue;
                    }

                    foreach (object entry in entryEnumerable)
                    {
                        if (entry == null)
                        {
                            continue;
                        }

                        builtAnyEntries = true;

                        if (IsMatchingEntry(entry, profile))
                        {
                            matchingEntry = entry;
                            break;
                        }
                    }

                    if (matchingEntry != null)
                    {
                        break;
                    }
                }
            }

            // Compatibilidad defensiva: si una versión futura deja de exponer
            // categories/buildEntries como hoy, conservamos el antiguo escaneo
            // estático como fallback. Nunca se usa para sobreescribir un match
            // obtenido del camino nativo anterior.
            if (matchingEntry == null && !categoriesResolved)
            {
                bool inspectedEntryContainer = false;

                matchingEntry =
                    TryFindMatchingEntryInStaticMembers(
                        controllerType,
                        profile,
                        ref inspectedEntryContainer
                    );

                if (
                    matchingEntry == null &&
                    inspectedEntryContainer
                )
                {
                    result.State = NativeEntryState.Missing;
                    return result;
                }
            }

            if (matchingEntry == null)
            {
                // Si el juego construyó entradas válidas y ninguna corresponde
                // al survivor, podemos afirmar que no tiene entrada nativa.
                // Si todavía no se construyó nada, fallamos cerrado (Unknown)
                // para no revelar lore durante una inicialización incompleta.
                if (categoriesResolved && builtAnyEntries)
                {
                    result.State = NativeEntryState.Missing;
                }

                return result;
            }

            object status =
                TryResolveEntryStatus(
                    matchingEntry,
                    userProfile
                );

            if (status == null)
            {
                return result;
            }

            string statusName = status.ToString() ?? "";
            result.StatusName = statusName;

            if (
                string.Equals(
                    statusName,
                    "Available",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                string.Equals(
                    statusName,
                    "New",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                result.State = NativeEntryState.Available;
                return result;
            }

            if (
                string.Equals(
                    statusName,
                    "Locked",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                string.Equals(
                    statusName,
                    "Unencountered",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                string.Equals(
                    statusName,
                    "None",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                string.Equals(
                    statusName,
                    "Unimplemented",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                result.State = NativeEntryState.Locked;
                return result;
            }

            return result;
        }


        private static object TryBuildCategoryEntries(
            object category,
            UserProfile userProfile
        )
        {
            if (category == null || userProfile == null)
            {
                return null;
            }

            Type categoryType = category.GetType();

            // En el juego actual buildEntries es un delegate almacenado en la
            // Category. Lo buscamos por nombre primero para evitar invocar por
            // accidente otros delegates de la categoría.
            object buildEntries =
                ReadMember(
                    category,
                    "buildEntries"
                );

            if (buildEntries is Delegate buildDelegate)
            {
                object built =
                    TryInvokeSingleProfileDelegate(
                        buildDelegate,
                        userProfile,
                        false
                    );

                if (built != null)
                {
                    return built;
                }
            }

            BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.IgnoreCase;

            // Fallback para una versión donde buildEntries sea método.
            MethodInfo[] methods = categoryType.GetMethods(flags);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];

                if (
                    method == null ||
                    !string.Equals(
                        method.Name,
                        "buildEntries",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }

                ParameterInfo[] parameters =
                    method.GetParameters();

                if (
                    parameters.Length != 1 ||
                    !parameters[0].ParameterType.IsInstanceOfType(
                        userProfile
                    )
                )
                {
                    continue;
                }

                try
                {
                    return method.Invoke(
                        category,
                        new object[] { userProfile }
                    );
                }
                catch
                {
                    // Probar la siguiente forma disponible.
                }
            }

            return null;
        }


        private static object TryFindMatchingEntryInStaticMembers(
            Type controllerType,
            SurvivorContentProfile profile,
            ref bool inspectedEntryContainer
        )
        {
            if (controllerType == null || profile == null)
            {
                return null;
            }

            BindingFlags flags =
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            FieldInfo[] fields = controllerType.GetFields(flags);

            for (int i = 0; i < fields.Length; i++)
            {
                object value = SafeGetStaticValue(fields[i]);

                if (
                    TryFindMatchingEntryRecursive(
                        value,
                        profile,
                        ref inspectedEntryContainer,
                        0,
                        out object matchingEntry
                    )
                )
                {
                    return matchingEntry;
                }
            }

            PropertyInfo[] properties =
                controllerType.GetProperties(flags);

            for (int i = 0; i < properties.Length; i++)
            {
                object value = SafeGetStaticValue(properties[i]);

                if (
                    TryFindMatchingEntryRecursive(
                        value,
                        profile,
                        ref inspectedEntryContainer,
                        0,
                        out object matchingEntry
                    )
                )
                {
                    return matchingEntry;
                }
            }

            return null;
        }


        private static bool TryFindMatchingEntryRecursive(
            object candidate,
            SurvivorContentProfile profile,
            ref bool inspectedEntryContainer,
            int depth,
            out object matchingEntry
        )
        {
            matchingEntry = null;

            if (
                candidate == null ||
                candidate is string ||
                depth > 4
            )
            {
                return false;
            }

            if (IsMatchingEntry(candidate, profile))
            {
                inspectedEntryContainer = true;
                matchingEntry = candidate;
                return true;
            }

            if (LooksLikeLogbookEntry(candidate))
            {
                inspectedEntryContainer = true;
                return false;
            }

            if (candidate is UnityEngine.Object)
            {
                return false;
            }

            if (candidate is IDictionary dictionary)
            {
                int inspectedDictionaryItems = 0;

                foreach (DictionaryEntry dictionaryEntry in dictionary)
                {
                    if (
                        TryFindMatchingEntryRecursive(
                            dictionaryEntry.Value,
                            profile,
                            ref inspectedEntryContainer,
                            depth + 1,
                            out matchingEntry
                        )
                    )
                    {
                        return true;
                    }

                    inspectedDictionaryItems++;

                    if (inspectedDictionaryItems > 4096)
                    {
                        break;
                    }
                }

                return false;
            }

            if (!(candidate is IEnumerable enumerable))
            {
                return false;
            }

            int inspected = 0;

            foreach (object item in enumerable)
            {
                if (item == null)
                {
                    continue;
                }

                object value = ReadMember(item, "Value");

                if (
                    value != null &&
                    !ReferenceEquals(value, item)
                )
                {
                    if (
                        TryFindMatchingEntryRecursive(
                            value,
                            profile,
                            ref inspectedEntryContainer,
                            depth + 1,
                            out matchingEntry
                        )
                    )
                    {
                        return true;
                    }
                }
                else if (
                    TryFindMatchingEntryRecursive(
                        item,
                        profile,
                        ref inspectedEntryContainer,
                        depth + 1,
                        out matchingEntry
                    )
                )
                {
                    return true;
                }

                inspected++;

                if (inspected > 4096)
                {
                    break;
                }
            }

            return false;
        }


        private static bool LooksLikeLogbookEntry(object candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            Type type = candidate.GetType();

            return
                FindMember(type, "extraData") != null ||
                type.Name.IndexOf(
                    "Entry",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;
        }


        private static bool IsMatchingEntry(
            object candidate,
            SurvivorContentProfile profile
        )
        {
            if (candidate == null || profile == null)
            {
                return false;
            }

            object extraData =
                ReadMember(candidate, "extraData");

            if (extraData == null)
            {
                return false;
            }

            if (
                extraData is SurvivorDef survivorDef &&
                survivorDef == profile.SurvivorDef
            )
            {
                return true;
            }

            if (extraData is CharacterBody body)
            {
                if (body.gameObject == profile.BodyPrefab)
                {
                    return true;
                }

                // Algunas construcciones del Logbook pueden devolver otra
                // referencia de CharacterBody para el mismo BodyCatalog.
                // BodyIndex permite reconocerlo sin depender de la instancia.
                CharacterBody profileBody =
                    profile.BodyPrefab != null
                        ? profile.BodyPrefab.GetComponent<CharacterBody>()
                        : null;

                if (
                    profileBody != null &&
                    (int)body.bodyIndex >= 0 &&
                    (int)profileBody.bodyIndex >= 0 &&
                    body.bodyIndex == profileBody.bodyIndex
                )
                {
                    return true;
                }
            }

            if (
                extraData is GameObject gameObject &&
                gameObject == profile.BodyPrefab
            )
            {
                return true;
            }

            Component component = extraData as Component;

            return
                component != null &&
                component.gameObject == profile.BodyPrefab;
        }


        private static object TryResolveEntryStatus(
            object entry,
            UserProfile userProfile
        )
        {
            if (entry == null || userProfile == null)
            {
                return null;
            }

            Type entryType = entry.GetType();

            // Primero nombres conocidos/semánticos. Así evitamos tomar un
            // delegate enum ajeno si una Entry añade más callbacks.
            string[] preferredMembers =
            {
                "getStatus",
                "getEntryStatus",
                "statusGetter",
                "getStatusForUser"
            };

            for (int i = 0; i < preferredMembers.Length; i++)
            {
                object member =
                    ReadMember(
                        entry,
                        preferredMembers[i]
                    );

                if (member is Delegate preferredDelegate)
                {
                    object value =
                        TryInvokeSingleProfileDelegate(
                            preferredDelegate,
                            userProfile,
                            true
                        );

                    if (value != null)
                    {
                        return value;
                    }
                }
            }

            BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            FieldInfo[] fields = entryType.GetFields(flags);

            for (int i = 0; i < fields.Length; i++)
            {
                object value;

                try
                {
                    value = fields[i].GetValue(entry);
                }
                catch
                {
                    continue;
                }

                if (value is Delegate del)
                {
                    object status =
                        TryInvokeSingleProfileDelegate(
                            del,
                            userProfile,
                            true
                        );

                    if (status != null)
                    {
                        return status;
                    }
                }
            }

            PropertyInfo[] properties =
                entryType.GetProperties(flags);

            for (int i = 0; i < properties.Length; i++)
            {
                if (
                    !properties[i].CanRead ||
                    properties[i].GetIndexParameters().Length != 0
                )
                {
                    continue;
                }

                object value;

                try
                {
                    value = properties[i].GetValue(entry, null);
                }
                catch
                {
                    continue;
                }

                if (value is Delegate del)
                {
                    object status =
                        TryInvokeSingleProfileDelegate(
                            del,
                            userProfile,
                            true
                        );

                    if (status != null)
                    {
                        return status;
                    }
                }
            }

            MethodInfo[] methods = entryType.GetMethods(flags);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];

                if (
                    method == null ||
                    method.ReturnType == typeof(void) ||
                    !method.ReturnType.IsEnum
                )
                {
                    continue;
                }

                ParameterInfo[] parameters =
                    method.GetParameters();

                if (
                    parameters.Length == 1 &&
                    parameters[0].ParameterType.IsInstanceOfType(
                        userProfile
                    )
                )
                {
                    try
                    {
                        return method.Invoke(
                            entry,
                            new object[] { userProfile }
                        );
                    }
                    catch
                    {
                        // Probar otro candidato.
                    }
                }
            }

            return null;
        }


        private static object TryInvokeSingleProfileDelegate(
            Delegate del,
            UserProfile userProfile,
            bool requireEnumResult
        )
        {
            if (del == null || userProfile == null)
            {
                return null;
            }

            ParameterInfo[] parameters =
                del.Method.GetParameters();

            if (
                parameters.Length != 1 ||
                !parameters[0].ParameterType.IsInstanceOfType(
                    userProfile
                )
            )
            {
                return null;
            }

            try
            {
                object value = del.DynamicInvoke(userProfile);

                if (value == null)
                {
                    return null;
                }

                if (
                    requireEnumResult &&
                    !value.GetType().IsEnum
                )
                {
                    return null;
                }

                return value;
            }
            catch
            {
                return null;
            }
        }


        private static Type FindType(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return null;
            }

            Assembly[] assemblies =
                AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                try
                {
                    Type type = assemblies[i].GetType(
                        fullName,
                        false
                    );

                    if (type != null)
                    {
                        return type;
                    }
                }
                catch
                {
                    // Continuar con el resto de assemblies.
                }
            }

            return null;
        }


        private static object ReadStaticMember(
            Type type,
            string memberName
        )
        {
            if (
                type == null ||
                string.IsNullOrWhiteSpace(memberName)
            )
            {
                return null;
            }

            BindingFlags flags =
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.IgnoreCase;

            FieldInfo field =
                type.GetField(memberName, flags);

            if (field != null)
            {
                return SafeGetStaticValue(field);
            }

            PropertyInfo property =
                type.GetProperty(memberName, flags);

            return SafeGetStaticValue(property);
        }


        private static object SafeGetStaticValue(FieldInfo field)
        {
            if (field == null)
            {
                return null;
            }

            try
            {
                return field.GetValue(null);
            }
            catch
            {
                return null;
            }
        }


        private static object SafeGetStaticValue(PropertyInfo property)
        {
            if (
                property == null ||
                !property.CanRead ||
                property.GetIndexParameters().Length != 0
            )
            {
                return null;
            }

            try
            {
                return property.GetValue(null, null);
            }
            catch
            {
                return null;
            }
        }


        private static MemberInfo FindMember(
            Type type,
            string memberName
        )
        {
            if (type == null)
            {
                return null;
            }

            BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.IgnoreCase;

            FieldInfo field =
                type.GetField(memberName, flags);

            if (field != null)
            {
                return field;
            }

            return type.GetProperty(memberName, flags);
        }


        private static object ReadMember(
            object instance,
            string memberName
        )
        {
            if (
                instance == null ||
                string.IsNullOrWhiteSpace(memberName)
            )
            {
                return null;
            }

            MemberInfo member =
                FindMember(
                    instance.GetType(),
                    memberName
                );

            try
            {
                if (member is FieldInfo field)
                {
                    return field.GetValue(instance);
                }

                if (
                    member is PropertyInfo property &&
                    property.CanRead &&
                    property.GetIndexParameters().Length == 0
                )
                {
                    return property.GetValue(instance, null);
                }
            }
            catch
            {
                // Miembro no compatible con esta versión.
            }

            return null;
        }


        private static UserProfile TryGetLocalProfile()
        {
            try
            {
                LocalUser localUser =
                    LocalUserManager.GetFirstLocalUser();

                if (localUser != null)
                {
                    return localUser.userProfile;
                }
            }
            catch
            {
                // El perfil todavía puede no existir al abrir la UI.
            }

            try
            {
                if (
                    LocalUserManager.readOnlyLocalUsersList != null &&
                    LocalUserManager.readOnlyLocalUsersList.Count > 0
                )
                {
                    return
                        LocalUserManager
                            .readOnlyLocalUsersList[0]
                            ?.userProfile;
                }
            }
            catch
            {
                // Sin perfil local disponible.
            }

            return null;
        }
    }
}
