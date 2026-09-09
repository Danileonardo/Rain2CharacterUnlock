using System;
using System.Collections.Generic;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// 5G.2E-B.1 - Orden visual del navegador de survivors.
    ///
    /// Risk of Rain 2 y mods de ordenamiento utilizan
    /// SurvivorDef.desiredSortPosition para construir el orden efectivo
    /// del Character Select. USU respeta ese valor, pero conserva la regla
    /// del proyecto de mostrar primero contenido Vanilla/DLC y después los
    /// survivors modded.
    /// </summary>
    internal static class SurvivorBrowserOrderResolver
    {
        public static void Sort(
            List<SurvivorContentProfile> profiles
        )
        {
            if (profiles == null)
            {
                return;
            }

            profiles.Sort(Compare);
        }

        private static int Compare(
            SurvivorContentProfile left,
            SurvivorContentProfile right
        )
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            int groupCompare =
                GetSourceGroup(left)
                    .CompareTo(GetSourceGroup(right));

            if (groupCompare != 0)
            {
                return groupCompare;
            }

            float leftSort = GetDesiredSortPosition(left);
            float rightSort = GetDesiredSortPosition(right);

            int sortCompare = leftSort.CompareTo(rightSort);

            if (sortCompare != 0)
            {
                return sortCompare;
            }

            int indexCompare =
                GetSurvivorIndex(left)
                    .CompareTo(GetSurvivorIndex(right));

            if (indexCompare != 0)
            {
                return indexCompare;
            }

            return string.Compare(
                left.BodyName ?? "",
                right.BodyName ?? "",
                StringComparison.OrdinalIgnoreCase
            );
        }

        /// <summary>
        /// 0 = Vanilla/DLC del juego.
        /// 1 = contenido modded.
        /// 2 = fallback desconocido.
        ///
        /// No separamos Vanilla y DLC entre sí porque Character Select los
        /// mezcla mediante desiredSortPosition. Así se conserva el orden 2D
        /// real del selector al convertirlo en la lista vertical de USU.
        /// </summary>
        private static int GetSourceGroup(
            SurvivorContentProfile profile
        )
        {
            if (profile == null)
            {
                return 2;
            }

            ContentCatalogEntry entry;

            if (
                ContentCatalogService.TryGet(
                    "survivor:" + profile.BodyName,
                    out entry
                ) &&
                entry?.Source != null
            )
            {
                switch (entry.Source.Kind)
                {
                    case ContentCatalogSourceKind.Vanilla:
                    case ContentCatalogSourceKind.Dlc:
                        return 0;

                    case ContentCatalogSourceKind.Mod:
                    case ContentCatalogSourceKind.Usu:
                        return 1;
                }
            }

            if (profile.SurvivorInfo != null)
            {
                return profile.SurvivorInfo.IsModded
                    ? 1
                    : 0;
            }

            string source =
                (profile.SourceIdentifier ?? "") + " " +
                (profile.SourceAssembly ?? "");

            if (
                source.IndexOf(
                    "RoR2",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0 ||
                source.IndexOf(
                    "DLC",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0
            )
            {
                return 0;
            }

            return 2;
        }

        private static float GetDesiredSortPosition(
            SurvivorContentProfile profile
        )
        {
            SurvivorDef survivorDef = profile?.SurvivorDef;

            if (survivorDef == null)
            {
                return float.MaxValue;
            }

            try
            {
                return survivorDef.desiredSortPosition;
            }
            catch
            {
                return float.MaxValue;
            }
        }

        private static int GetSurvivorIndex(
            SurvivorContentProfile profile
        )
        {
            SurvivorDef survivorDef = profile?.SurvivorDef;

            if (survivorDef == null)
            {
                return int.MaxValue;
            }

            try
            {
                return (int)survivorDef.survivorIndex;
            }
            catch
            {
                return int.MaxValue;
            }
        }
    }
}
