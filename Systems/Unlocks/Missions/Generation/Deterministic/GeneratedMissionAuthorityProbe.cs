using System;
using System.Collections.Generic;
using System.Reflection;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Puente de sólo lectura entre G7 y la autoridad de misión ya existente.
    ///
    /// G7 sólo debe generar un fallback cuando NO existe contenido autorado.
    /// La fuente principal es Survivors.json (SurvivorJsonManager), porque
    /// SurvivorInfo describe detección runtime y no es la autoridad persistida
    /// de MissionConfig. El registro oficial actúa además como red de seguridad
    /// para que un preset oficial nunca sea reemplazado por Generated aunque un
    /// snapshot antiguo esté incompleto.
    /// </summary>
    public static class GeneratedMissionAuthorityProbe
    {
        public static GeneratedMissionAuthorityKind Detect(
            IReadOnlyList<SurvivorInfo> survivors,
            string survivorId,
            string bodyName,
            out string detail
        )
        {
            detail = "None";

            if (string.IsNullOrWhiteSpace(bodyName))
            {
                return GeneratedMissionAuthorityKind.None;
            }

            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(bodyName);

            if (
                entry == null &&
                !string.IsNullOrWhiteSpace(survivorId)
            )
            {
                entry =
                    SurvivorJsonManager.FindEntryBySurvivorId(
                        survivorId
                    );
            }

            if (UnlockProviderPolicy.HasStoredOriginalUnlock(entry))
            {
                detail =
                    "OriginalUnlock<" +
                    (entry?.OriginalUnlock ?? "") +
                    ">";

                return GeneratedMissionAuthorityKind.OriginalUnlock;
            }


            GeneratedMissionAuthorityKind persistedAuthority =
                DetectPersistedAuthority(
                    entry,
                    out detail
                );

            if (persistedAuthority != GeneratedMissionAuthorityKind.None)
            {
                return persistedAuthority;
            }

            /*
             * Red de seguridad para presets oficiales.
             *
             * Los nueve survivors oficiales sin unlock Original deben quedar
             * suprimidos aunque un snapshot preexistente haya perdido
             * MissionConfig/BasePresetId. El catálogo oficial es autorado por
             * USU, por lo que siempre tiene prioridad sobre Generated.
             */
            string contentPackIdentifier =
                entry?.Source ?? "";

            if (string.IsNullOrWhiteSpace(contentPackIdentifier))
            {
                contentPackIdentifier =
                    FindContentPackIdentifier(
                        survivors,
                        bodyName
                    );
            }

            if (
                OfficialMissionPresetRegistry.TryResolve(
                    bodyName,
                    contentPackIdentifier,
                    out OfficialMissionPresetDescriptor descriptor
                ) &&
                descriptor != null
            )
            {
                detail =
                    "OfficialRegistry<" +
                    (descriptor.PresetId ?? "") +
                    ">";

                return GeneratedMissionAuthorityKind.UsuOfficialPreset;
            }

            /*
             * Compatibilidad defensiva futura: si SurvivorInfo llega a exponer
             * Challenge/MissionConfig, también lo reconocemos. No se considera
             * una Mission desnuda como autorada, porque podría ser un fallback
             * antiguo sin metadata y eso bloquearía precisamente a Gwen.
             */
            return DetectRuntimeSnapshot(
                survivors,
                bodyName,
                out detail
            );
        }


        private static GeneratedMissionAuthorityKind DetectPersistedAuthority(
            SurvivorJsonEntry entry,
            out string detail
        )
        {
            detail = "None";

            SurvivorChallengeJson challenge =
                entry?.Challenge;

            MissionConfiguration missionConfig =
                challenge?.MissionConfig;

            if (missionConfig == null)
            {
                return GeneratedMissionAuthorityKind.None;
            }

            string source =
                missionConfig.Source ?? "";

            string basePresetId =
                missionConfig.BasePresetId ?? "";

            // Source=Generated es persistente pero sigue siendo fallback
            // administrado por G7/G8. Debe poder recalcularse en cada arranque.
            if (
                string.Equals(
                    source,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                detail = "GeneratedManagedFallback";
                return GeneratedMissionAuthorityKind.None;
            }

            if (
                string.Equals(
                    source,
                    "Custom",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                detail =
                    "ActiveCustom" +
                    (!string.IsNullOrWhiteSpace(basePresetId)
                        ? "<" + basePresetId + ">"
                        : "");

                return GeneratedMissionAuthorityKind.UsuCustom;
            }

            if (
                string.Equals(
                    source,
                    "Preset",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                !string.IsNullOrWhiteSpace(basePresetId)
            )
            {
                detail = "Preset<" + basePresetId + ">";
                return GeneratedMissionAuthorityKind.UsuOfficialPreset;
            }

            if (!string.IsNullOrWhiteSpace(basePresetId))
            {
                detail = "PresetMetadata<" + basePresetId + ">";
                return GeneratedMissionAuthorityKind.UsuOfficialPreset;
            }

            /*
             * Una CUSTOM dormida también es contenido autorado. Si no hay un
             * preset activo por encima, Generated no debe competir con ella.
             */
            if (missionConfig.CustomMission != null)
            {
                detail = "DormantCustomMission";
                return GeneratedMissionAuthorityKind.UsuCustom;
            }

            /*
             * Sólo aceptamos una Mission ya materializada como autorada cuando
             * MissionConfig también declara alguna Source. Una Mission desnuda
             * sin metadata puede proceder de compatibilidad/fallback legacy.
             */
            if (
                challenge != null &&
                challenge.Enabled &&
                challenge.Mission != null &&
                !string.IsNullOrWhiteSpace(source)
            )
            {
                detail = "ExistingChallengeMission<" + source + ">";
                return GeneratedMissionAuthorityKind.ExistingUsuMission;
            }

            return GeneratedMissionAuthorityKind.None;
        }


        private static GeneratedMissionAuthorityKind DetectRuntimeSnapshot(
            IReadOnlyList<SurvivorInfo> survivors,
            string bodyName,
            out string detail
        )
        {
            detail = "None";

            if (survivors == null)
            {
                return GeneratedMissionAuthorityKind.None;
            }

            for (int i = 0; i < survivors.Count; i++)
            {
                SurvivorInfo survivor = survivors[i];

                if (survivor == null)
                {
                    continue;
                }

                string candidateBody =
                    ReadString(survivor, "BodyName");

                if (string.IsNullOrWhiteSpace(candidateBody))
                {
                    object bodyPrefab = ReadMember(survivor, "BodyPrefab");

                    if (bodyPrefab != null)
                    {
                        candidateBody = ReadString(bodyPrefab, "name");
                    }
                }

                if (!string.Equals(
                    candidateBody,
                    bodyName,
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    continue;
                }

                object challenge = ReadMember(survivor, "Challenge");
                object missionConfig = ReadMember(challenge, "MissionConfig");

                if (missionConfig == null)
                {
                    return GeneratedMissionAuthorityKind.None;
                }

                object customMission =
                    ReadMember(missionConfig, "CustomMission");

                string source =
                    ReadString(missionConfig, "Source");

                string basePresetId =
                    ReadString(missionConfig, "BasePresetId");

                if (Contains(source, "generated"))
                {
                    detail = "RuntimeGeneratedManagedFallback";
                    return GeneratedMissionAuthorityKind.None;
                }

                if (Contains(source, "custom"))
                {
                    detail = "RuntimeCustom";
                    return GeneratedMissionAuthorityKind.UsuCustom;
                }

                if (!string.IsNullOrWhiteSpace(basePresetId))
                {
                    detail = "RuntimePreset<" + basePresetId + ">";
                    return GeneratedMissionAuthorityKind.UsuOfficialPreset;
                }

                if (customMission != null)
                {
                    detail = "RuntimeDormantCustom";
                    return GeneratedMissionAuthorityKind.UsuCustom;
                }

                return GeneratedMissionAuthorityKind.None;
            }

            return GeneratedMissionAuthorityKind.None;
        }


        private static string FindContentPackIdentifier(
            IReadOnlyList<SurvivorInfo> survivors,
            string bodyName
        )
        {
            if (survivors == null)
            {
                return "";
            }

            for (int i = 0; i < survivors.Count; i++)
            {
                SurvivorInfo survivor = survivors[i];

                if (survivor == null)
                {
                    continue;
                }

                string candidateBody =
                    ReadString(survivor, "BodyName");

                if (!string.Equals(
                    candidateBody,
                    bodyName,
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    continue;
                }

                string contentPack =
                    ReadString(survivor, "ContentPackIdentifier");

                if (string.IsNullOrWhiteSpace(contentPack))
                {
                    contentPack = ReadString(survivor, "Source");
                }

                if (string.IsNullOrWhiteSpace(contentPack))
                {
                    contentPack = ReadString(survivor, "SourceAssembly");
                }

                return contentPack ?? "";
            }

            return "";
        }


        private static bool Contains(
            string value,
            string fragment
        )
        {
            return
                !string.IsNullOrWhiteSpace(value) &&
                value.IndexOf(
                    fragment,
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;
        }


        private static string ReadString(
            object instance,
            string memberName
        )
        {
            object value = ReadMember(instance, memberName);
            return value != null ? value.ToString() : "";
        }


        private static object ReadMember(
            object instance,
            string memberName
        )
        {
            if (instance == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            Type type = instance.GetType();

            BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.IgnoreCase;

            try
            {
                PropertyInfo property =
                    type.GetProperty(memberName, flags);

                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    return property.GetValue(instance, null);
                }
            }
            catch
            {
                // Sólo diagnóstico. Continuamos intentando field.
            }

            try
            {
                FieldInfo field =
                    type.GetField(memberName, flags);

                if (field != null)
                {
                    return field.GetValue(instance);
                }
            }
            catch
            {
                // Sólo diagnóstico. Un layout desconocido equivale a None.
            }

            return null;
        }
    }
}
