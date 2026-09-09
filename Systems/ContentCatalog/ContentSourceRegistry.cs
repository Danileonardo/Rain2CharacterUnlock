using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using BepInEx.Logging;
using R2API.ContentManagement;
using RoR2.ContentManagement;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Relaciona assets runtime con el ContentPack que los aportó.
    ///
    /// Se alimenta durante GenerateContentPackAsync, cuando USU ya recibe
    /// peerLoadInfos de todos los providers. Usa reflexión únicamente para
    /// recorrer las colecciones del ReadOnlyContentPack, de modo que no
    /// tengamos que mantener una lista manual de itemDefs/skillDefs/etc.
    /// </summary>
    public static class ContentSourceRegistry
    {
        private const string UsuPluginGuid =
            "com.danileo.UniversalSurvivorUnlocks";

        private static readonly Dictionary<
            UnityEngine.Object,
            ContentCatalogSourceInfo
        > Sources =
            new Dictionary<
                UnityEngine.Object,
                ContentCatalogSourceInfo
            >();

        private static readonly Dictionary<
            string,
            ContentCatalogSourceInfo
        > SourcesByAssetName =
            new Dictionary<
                string,
                ContentCatalogSourceInfo
            >(
                StringComparer.OrdinalIgnoreCase
            );

        public static int Count =>
            Sources.Count;

        public static void Reset(
            ManualLogSource logger
        )
        {
            Sources.Clear();
            SourcesByAssetName.Clear();

            UsuLog.Verbose(
                logger,
                "[CONTENT CATALOG] Registro de procedencia reiniciado."
            );
        }

        public static void RegisterContentPack(
            ReadOnlyContentPack contentPack,
            ManualLogSource logger
        )
        {
            string identifier =
                string.IsNullOrWhiteSpace(contentPack.identifier)
                    ? "UnknownContentPack"
                    : contentPack.identifier;

            ContentCatalogSourceInfo source =
                new ContentCatalogSourceInfo
                {
                    ContentPackIdentifier =
                        identifier,

                    AssemblyName =
                        ResolveAssemblyName(identifier),

                    Kind =
                        ResolveSourceKind(identifier)
                };

            int before =
                Sources.Count;

            try
            {
                PropertyInfo[] properties =
                    typeof(ReadOnlyContentPack)
                        .GetProperties(
                            BindingFlags.Public |
                            BindingFlags.Instance
                        );

                for (
                    int i = 0;
                    i < properties.Length;
                    i++
                )
                {
                    PropertyInfo property =
                        properties[i];

                    if (
                        property == null ||
                        !property.CanRead
                    )
                    {
                        continue;
                    }

                    object value;

                    try
                    {
                        value =
                            property.GetValue(
                                contentPack,
                                null
                            );
                    }
                    catch
                    {
                        continue;
                    }

                    if (
                        value == null ||
                        value is string ||
                        !(value is IEnumerable enumerable)
                    )
                    {
                        continue;
                    }

                    foreach (
                        object element
                        in enumerable
                    )
                    {
                        if (
                            element is UnityEngine.Object asset &&
                            asset != null
                        )
                        {
                            RegisterAsset(
                                asset,
                                source
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(
                    "[CONTENT CATALOG] No se pudo recorrer completamente " +
                    "el ContentPack '" +
                    identifier +
                    "': " +
                    ex.GetType().Name +
                    " - " +
                    ex.Message
                );
            }

            int added =
                Sources.Count - before;

            UsuLog.Verbose(
                logger,
                "[CONTENT CATALOG] ContentPack registrado | " +
                "Pack: " +
                identifier +
                " | Fuente: " +
                source.Kind +
                " | Assets nuevos: " +
                added
            );
        }

        private static void RegisterAsset(
            UnityEngine.Object asset,
            ContentCatalogSourceInfo source
        )
        {
            if (
                asset == null ||
                source == null
            )
            {
                return;
            }

            if (!Sources.ContainsKey(asset))
            {
                Sources.Add(
                    asset,
                    source
                );
            }

            if (
                !string.IsNullOrWhiteSpace(asset.name) &&
                !SourcesByAssetName.ContainsKey(asset.name)
            )
            {
                SourcesByAssetName.Add(
                    asset.name,
                    source
                );
            }
        }

        public static bool TryGetSource(
            UnityEngine.Object asset,
            out ContentCatalogSourceInfo source
        )
        {
            source =
                null;

            if (asset == null)
            {
                return false;
            }

            if (
                Sources.TryGetValue(
                    asset,
                    out source
                )
            )
            {
                return true;
            }

            if (
                !string.IsNullOrWhiteSpace(asset.name) &&
                SourcesByAssetName.TryGetValue(
                    asset.name,
                    out source
                )
            )
            {
                return true;
            }

            return false;
        }

        private static ContentCatalogSourceKind ResolveSourceKind(
            string identifier
        )
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return ContentCatalogSourceKind.Unknown;
            }

            if (
                identifier.StartsWith(
                    UsuPluginGuid,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return ContentCatalogSourceKind.Usu;
            }

            if (
                identifier.StartsWith(
                    "RoR2.DLC",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return ContentCatalogSourceKind.Dlc;
            }

            if (
                identifier.StartsWith(
                    "RoR2.",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return ContentCatalogSourceKind.Vanilla;
            }

            return ContentCatalogSourceKind.Mod;
        }

        private static string ResolveAssemblyName(
            string identifier
        )
        {
            try
            {
                var managedPacks =
                    R2APIContentManager.ManagedContentPacks;

                foreach (
                    ManagedReadOnlyContentPack managedPack
                    in managedPacks
                )
                {
                    if (
                        !string.Equals(
                            managedPack.Identifier,
                            identifier,
                            StringComparison.Ordinal
                        )
                    )
                    {
                        continue;
                    }

                    if (managedPack.TiedAssembly != null)
                    {
                        return managedPack
                            .TiedAssembly
                            .GetName()
                            .Name;
                    }
                }
            }
            catch
            {
                // Metadata opcional. Nunca debe bloquear el catálogo.
            }

            return identifier;
        }
    }
}
