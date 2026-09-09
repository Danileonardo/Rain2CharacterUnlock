using System;
using System.Reflection;

using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    internal static class ContentCatalogReflection
    {
        public static string ReadString(
            object target,
            params string[] memberNames
        )
        {
            if (
                target == null ||
                memberNames == null
            )
            {
                return "";
            }

            Type type =
                target.GetType();

            for (
                int i = 0;
                i < memberNames.Length;
                i++
            )
            {
                string name =
                    memberNames[i];

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                try
                {
                    PropertyInfo property =
                        type.GetProperty(
                            name,
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Instance
                        );

                    if (
                        property != null &&
                        property.CanRead &&
                        property.PropertyType == typeof(string)
                    )
                    {
                        return
                            property.GetValue(
                                target,
                                null
                            ) as string ?? "";
                    }

                    FieldInfo field =
                        type.GetField(
                            name,
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Instance
                        );

                    if (
                        field != null &&
                        field.FieldType == typeof(string)
                    )
                    {
                        return
                            field.GetValue(target) as string ?? "";
                    }
                }
                catch
                {
                    // Continuar con el siguiente alias.
                }
            }

            return "";
        }

        public static T ReadUnityObject<T>(
            object target,
            params string[] memberNames
        )
            where T : UnityEngine.Object
        {
            if (
                target == null ||
                memberNames == null
            )
            {
                return null;
            }

            Type type =
                target.GetType();

            for (
                int i = 0;
                i < memberNames.Length;
                i++
            )
            {
                string name =
                    memberNames[i];

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                try
                {
                    PropertyInfo property =
                        type.GetProperty(
                            name,
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Instance
                        );

                    if (
                        property != null &&
                        property.CanRead &&
                        typeof(T).IsAssignableFrom(
                            property.PropertyType
                        )
                    )
                    {
                        return
                            property.GetValue(
                                target,
                                null
                            ) as T;
                    }

                    FieldInfo field =
                        type.GetField(
                            name,
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Instance
                        );

                    if (
                        field != null &&
                        typeof(T).IsAssignableFrom(
                            field.FieldType
                        )
                    )
                    {
                        return
                            field.GetValue(target) as T;
                    }
                }
                catch
                {
                    // Continuar con el siguiente alias.
                }
            }

            return null;
        }

        public static bool ReadBool(
            object target,
            bool fallback,
            params string[] memberNames
        )
        {
            if (
                target == null ||
                memberNames == null
            )
            {
                return fallback;
            }

            Type type =
                target.GetType();

            for (
                int i = 0;
                i < memberNames.Length;
                i++
            )
            {
                string name =
                    memberNames[i];

                try
                {
                    PropertyInfo property =
                        type.GetProperty(
                            name,
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Instance
                        );

                    if (
                        property != null &&
                        property.CanRead &&
                        property.PropertyType == typeof(bool)
                    )
                    {
                        return (bool)property.GetValue(
                            target,
                            null
                        );
                    }

                    FieldInfo field =
                        type.GetField(
                            name,
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Instance
                        );

                    if (
                        field != null &&
                        field.FieldType == typeof(bool)
                    )
                    {
                        return (bool)field.GetValue(target);
                    }
                }
                catch
                {
                    // Continuar con el siguiente alias.
                }
            }

            return fallback;
        }

        public static string ResolveDisplayName(
            UnityEngine.Object asset,
            params string[] tokenMembers
        )
        {
            if (asset == null)
            {
                return "";
            }

            string token =
                ReadString(
                    asset,
                    tokenMembers
                );

            if (!string.IsNullOrWhiteSpace(token))
            {
                string localized =
                    Language.GetString(token);

                if (
                    !string.IsNullOrWhiteSpace(localized) &&
                    !string.Equals(
                        localized,
                        token,
                        StringComparison.Ordinal
                    )
                )
                {
                    return localized;
                }
            }

            return
                string.IsNullOrWhiteSpace(asset.name)
                    ? "Unknown"
                    : asset.name;
        }

        public static string ResolveDescription(
            UnityEngine.Object asset,
            params string[] tokenMembers
        )
        {
            if (asset == null)
            {
                return "";
            }

            string token =
                ReadString(
                    asset,
                    tokenMembers
                );

            if (string.IsNullOrWhiteSpace(token))
            {
                return "";
            }

            string localized =
                Language.GetString(token);

            if (
                string.IsNullOrWhiteSpace(localized) ||
                string.Equals(
                    localized,
                    token,
                    StringComparison.Ordinal
                )
            )
            {
                return "";
            }

            return localized;
        }
    }
}
