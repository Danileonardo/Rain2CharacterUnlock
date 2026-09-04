using System;
using System.Collections.Generic;
using System.Reflection;

using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Catálogo de efectos de estado disponibles para presets/editor.
    ///
    /// Reutiliza el StatusEffectScanner ya existente para buffs y
    /// debuffs, y descubre los DotIndex registrados por el juego
    /// mediante reflexión para incluir Poison, Bleed, Burn, etc.
    ///
    /// No codifica una lista cerrada de mods: si un mod registra un
    /// BuffDef válido, aparecerá automáticamente después de Rebuild.
    /// </summary>
    public static class MissionStatusEffectCatalog
    {
        public static List<MissionStatusEffectDescriptor> GetAll()
        {
            Dictionary<string, MissionStatusEffectDescriptor> result =
                new Dictionary<string, MissionStatusEffectDescriptor>(
                    StringComparer.OrdinalIgnoreCase
                );


            AddBuffs(
                result,
                StatusEffectScanner.GetNegativeBuffsSnapshot(),
                "NegativeBuff",
                false,
                true
            );


            AddBuffs(
                result,
                StatusEffectScanner.GetPositiveBuffsSnapshot(),
                "PositiveBuff",
                true,
                false
            );


            AddDots(
                result
            );


            // Freeze/Stun se rastrean actualmente como estados de CC
            // y no necesariamente como BuffDef persistentes.
            AddDescriptor(
                result,
                new MissionStatusEffectDescriptor
                {
                    Id = "cc:Freeze",
                    Family = "CrowdControl",
                    InternalName = "Freeze",
                    DisplayName = "Freeze",
                    IsNegative = true
                }
            );


            AddDescriptor(
                result,
                new MissionStatusEffectDescriptor
                {
                    Id = "cc:Stun",
                    Family = "CrowdControl",
                    InternalName = "Stun",
                    DisplayName = "Stun",
                    IsNegative = true
                }
            );


            return new List<MissionStatusEffectDescriptor>(
                result.Values
            );
        }


        public static bool TryGet(
            string statusId,
            out MissionStatusEffectDescriptor descriptor
        )
        {
            descriptor =
                null;


            if (
                string.IsNullOrWhiteSpace(
                    statusId
                )
            )
            {
                return false;
            }


            List<MissionStatusEffectDescriptor> all =
                GetAll();


            for (
                int i = 0;
                i < all.Count;
                i++
            )
            {
                MissionStatusEffectDescriptor candidate =
                    all[i];


                if (
                    candidate != null &&
                    string.Equals(
                        candidate.Id,
                        statusId.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    descriptor =
                        candidate;

                    return true;
                }
            }


            return false;
        }


        private static void AddBuffs(
            Dictionary<string, MissionStatusEffectDescriptor> result,
            IReadOnlyList<BuffDef> buffs,
            string family,
            bool isPositive,
            bool isNegative
        )
        {
            if (buffs == null)
            {
                return;
            }


            for (
                int i = 0;
                i < buffs.Count;
                i++
            )
            {
                BuffDef buffDef =
                    buffs[i];


                if (buffDef == null)
                {
                    continue;
                }


                string internalName =
                    !string.IsNullOrWhiteSpace(
                        buffDef.name
                    )
                        ? buffDef.name
                        : buffDef.buffIndex.ToString();


                AddDescriptor(
                    result,
                    new MissionStatusEffectDescriptor
                    {
                        Id =
                            "buff:" + internalName,

                        Family =
                            family,

                        InternalName =
                            internalName,

                        DisplayName =
                            internalName,

                        IsPositive =
                            isPositive,

                        IsNegative =
                            isNegative
                    }
                );
            }
        }


        private static void AddDots(
            Dictionary<string, MissionStatusEffectDescriptor> result
        )
        {
            try
            {
                Type dotIndexType =
                    typeof(DotController)
                        .GetNestedType(
                            "DotIndex",
                            BindingFlags.Public |
                            BindingFlags.NonPublic
                        );


                if (
                    dotIndexType == null ||
                    !dotIndexType.IsEnum
                )
                {
                    return;
                }


                MethodInfo getDotDef =
                    typeof(DotController)
                        .GetMethod(
                            "GetDotDef",
                            BindingFlags.Static |
                            BindingFlags.Public |
                            BindingFlags.NonPublic,
                            null,
                            new Type[]
                            {
                                dotIndexType
                            },
                            null
                        );


                if (getDotDef == null)
                {
                    AddEnumDotsOnly(
                        result,
                        dotIndexType
                    );

                    return;
                }


                /*
                 * Recorremos un rango razonable de índices en lugar
                 * de depender únicamente de Enum.GetValues().
                 *
                 * Esto permite descubrir también DotIndex agregados
                 * dinámicamente por APIs/mods cuando GetDotDef ya
                 * conoce su definición aunque el enum original no
                 * tenga un nombre compilado para ese valor.
                 */
                for (
                    int rawIndex = 0;
                    rawIndex < 512;
                    rawIndex++
                )
                {
                    object dotIndexValue =
                        Enum.ToObject(
                            dotIndexType,
                            rawIndex
                        );


                    object dotDef;


                    try
                    {
                        dotDef =
                            getDotDef.Invoke(
                                null,
                                new object[]
                                {
                                    dotIndexValue
                                }
                            );
                    }
                    catch
                    {
                        continue;
                    }


                    if (dotDef == null)
                    {
                        continue;
                    }


                    string enumName =
                        Enum.GetName(
                            dotIndexType,
                            dotIndexValue
                        );


                    string associatedBuffName =
                        GetAssociatedBuffName(
                            dotDef
                        );


                    string dotName =
                        !string.IsNullOrWhiteSpace(
                            enumName
                        )
                            ? enumName
                            : !string.IsNullOrWhiteSpace(
                                associatedBuffName
                            )
                                ? associatedBuffName
                                : $"DotIndex_{rawIndex}";


                    if (
                        string.Equals(
                            dotName,
                            "None",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        continue;
                    }


                    AddDescriptor(
                        result,
                        new MissionStatusEffectDescriptor
                        {
                            Id =
                                "dot:" + dotName,

                            Family =
                                "Dot",

                            InternalName =
                                dotName,

                            DisplayName =
                                dotName,

                            IsNegative =
                                true
                        }
                    );
                }
            }
            catch
            {
                // El catálogo de presets nunca debe impedir el arranque
                // del mod si una versión del juego cambia DotIndex.
            }
        }


        private static void AddEnumDotsOnly(
            Dictionary<string, MissionStatusEffectDescriptor> result,
            Type dotIndexType
        )
        {
            Array values =
                Enum.GetValues(
                    dotIndexType
                );


            for (
                int i = 0;
                i < values.Length;
                i++
            )
            {
                object value =
                    values.GetValue(i);


                string dotName =
                    value != null
                        ? value.ToString()
                        : "";


                if (
                    string.IsNullOrWhiteSpace(
                        dotName
                    ) ||
                    string.Equals(
                        dotName,
                        "None",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }


                AddDescriptor(
                    result,
                    new MissionStatusEffectDescriptor
                    {
                        Id =
                            "dot:" + dotName,

                        Family =
                            "Dot",

                        InternalName =
                            dotName,

                        DisplayName =
                            dotName,

                        IsNegative =
                            true
                    }
                );
            }
        }


        private static string GetAssociatedBuffName(
            object dotDef
        )
        {
            if (dotDef == null)
            {
                return "";
            }


            Type dotDefType =
                dotDef.GetType();


            FieldInfo field =
                dotDefType.GetField(
                    "associatedBuff",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );


            BuffDef buffDef =
                field != null
                    ? field.GetValue(dotDef) as BuffDef
                    : null;


            if (buffDef == null)
            {
                PropertyInfo property =
                    dotDefType.GetProperty(
                        "associatedBuff",
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic
                    );


                if (property != null)
                {
                    buffDef =
                        property.GetValue(
                            dotDef,
                            null
                        ) as BuffDef;
                }
            }


            if (buffDef == null)
            {
                return "";
            }


            return
                !string.IsNullOrWhiteSpace(
                    buffDef.name
                )
                    ? buffDef.name
                    : buffDef.buffIndex.ToString();
        }


        private static void AddDescriptor(
            Dictionary<string, MissionStatusEffectDescriptor> result,
            MissionStatusEffectDescriptor descriptor
        )
        {
            if (
                result == null ||
                descriptor == null ||
                string.IsNullOrWhiteSpace(
                    descriptor.Id
                )
            )
            {
                return;
            }


            result[
                descriptor.Id
            ] =
                descriptor;
        }
    }
}
