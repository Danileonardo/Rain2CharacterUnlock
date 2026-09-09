using System;
using System.Collections.Generic;

using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// 5G.2D-B
    ///
    /// Resuelve qué contenido conoce el perfil local.
    ///
    /// Regla importante:
    /// Discovery sólo controla QUÉ IDENTIDAD puede revelar el navegador.
    /// Nunca desactiva un tracker ni impide que una misión existente se
    /// ejecute en multiplayer.
    /// </summary>
    public static class ContentCatalogDiscoveryService
    {
        public static void Refresh(
            IReadOnlyList<ContentCatalogEntry> entries,
            ManualLogSource logger
        )
        {
            if (entries == null)
            {
                return;
            }

            UserProfile profile =
                TryGetLocalProfile();

            int discovered =
                0;

            int undiscovered =
                0;

            int notApplicable =
                0;

            int unknown =
                0;

            for (
                int i = 0;
                i < entries.Count;
                i++
            )
            {
                ContentCatalogEntry entry =
                    entries[i];

                if (entry == null)
                {
                    continue;
                }

                entry.Discovery =
                    ResolveDiscovery(
                        entry,
                        profile
                    );

                switch (entry.Discovery)
                {
                    case ContentCatalogDiscoveryState.Discovered:
                        discovered++;
                        break;

                    case ContentCatalogDiscoveryState.Undiscovered:
                        undiscovered++;
                        break;

                    case ContentCatalogDiscoveryState.NotApplicable:
                        notApplicable++;
                        break;

                    default:
                        unknown++;
                        break;
                }
            }

            UsuLog.Verbose(
                logger,
                "[CONTENT DISCOVERY] Refrescado | " +
                "Perfil: " +
                (profile != null ? "Sí" : "No") +
                " | Descubiertos: " +
                discovered +
                " | No descubiertos: " +
                undiscovered +
                " | No aplica: " +
                notApplicable +
                " | Desconocidos: " +
                unknown
            );
        }

        private static ContentCatalogDiscoveryState ResolveDiscovery(
            ContentCatalogEntry entry,
            UserProfile profile
        )
        {
            if (entry == null)
            {
                return
                    ContentCatalogDiscoveryState.Unknown;
            }

            if (profile == null)
            {
                return
                    ContentCatalogDiscoveryState.Unknown;
            }

            try
            {
                switch (entry.Kind)
                {
                    case ContentCatalogKind.Survivor:
                        return
                            ResolveSurvivorDiscovery(
                                entry,
                                profile
                            );

                    case ContentCatalogKind.Item:
                        return
                            ResolveItemDiscovery(
                                entry,
                                profile
                            );

                    case ContentCatalogKind.Equipment:
                        return
                            ResolveEquipmentDiscovery(
                                entry,
                                profile
                            );

                    case ContentCatalogKind.Enemy:
                    case ContentCatalogKind.Boss:
                    case ContentCatalogKind.Stage:
                        return
                            ResolveUnlockableDiscovery(
                                entry.DiscoveryUnlockable,
                                profile
                            );

                    /*
                     * Skills se resolverán de forma contextual en 5G.2D-C,
                     * porque el unlock real vive normalmente en
                     * SkillFamily.Variant y no en el SkillDef bruto.
                     */
                    case ContentCatalogKind.Skill:
                        return
                            ContentCatalogDiscoveryState.NotApplicable;

                    default:
                        return
                            ContentCatalogDiscoveryState.NotApplicable;
                }
            }
            catch
            {
                /*
                 * Un mod externo puede tener datos incompletos. El catálogo
                 * jamás debe romper la carga del juego por eso.
                 */
                return
                    ContentCatalogDiscoveryState.Unknown;
            }
        }

        private static ContentCatalogDiscoveryState ResolveSurvivorDiscovery(
            ContentCatalogEntry entry,
            UserProfile profile
        )
        {
            /*
             * Survivor sin unlock = visible por definición.
             */
            if (entry.DiscoveryUnlockable == null)
            {
                return
                    ContentCatalogDiscoveryState.Discovered;
            }

            return
                profile.HasUnlockable(
                    entry.DiscoveryUnlockable
                )
                    ? ContentCatalogDiscoveryState.Discovered
                    : ContentCatalogDiscoveryState.Undiscovered;
        }

        private static ContentCatalogDiscoveryState ResolveItemDiscovery(
            ContentCatalogEntry entry,
            UserProfile profile
        )
        {
            ItemDef item =
                entry.Asset as ItemDef;

            if (item == null)
            {
                return
                    ContentCatalogDiscoveryState.Unknown;
            }

            /*
             * Si existe un unlock de disponibilidad y el perfil aún no lo
             * posee, el contenido sigue siendo no descubierto.
             */
            if (
                entry.DiscoveryUnlockable != null &&
                !profile.HasUnlockable(
                    entry.DiscoveryUnlockable
                )
            )
            {
                return
                    ContentCatalogDiscoveryState.Undiscovered;
            }

            if ((int)item.itemIndex < 0)
            {
                return
                    ContentCatalogDiscoveryState.NotApplicable;
            }

            PickupIndex pickupIndex =
                PickupCatalog.FindPickupIndex(
                    item.itemIndex
                );

            if (pickupIndex == PickupIndex.none)
            {
                return
                    ContentCatalogDiscoveryState.NotApplicable;
            }

            return
                profile.HasDiscoveredPickup(
                    pickupIndex
                )
                    ? ContentCatalogDiscoveryState.Discovered
                    : ContentCatalogDiscoveryState.Undiscovered;
        }

        private static ContentCatalogDiscoveryState ResolveEquipmentDiscovery(
            ContentCatalogEntry entry,
            UserProfile profile
        )
        {
            EquipmentDef equipment =
                entry.Asset as EquipmentDef;

            if (equipment == null)
            {
                return
                    ContentCatalogDiscoveryState.Unknown;
            }

            if (
                entry.DiscoveryUnlockable != null &&
                !profile.HasUnlockable(
                    entry.DiscoveryUnlockable
                )
            )
            {
                return
                    ContentCatalogDiscoveryState.Undiscovered;
            }

            if ((int)equipment.equipmentIndex < 0)
            {
                return
                    ContentCatalogDiscoveryState.NotApplicable;
            }

            PickupIndex pickupIndex =
                PickupCatalog.FindPickupIndex(
                    equipment.equipmentIndex
                );

            if (pickupIndex == PickupIndex.none)
            {
                return
                    ContentCatalogDiscoveryState.NotApplicable;
            }

            return
                profile.HasDiscoveredPickup(
                    pickupIndex
                )
                    ? ContentCatalogDiscoveryState.Discovered
                    : ContentCatalogDiscoveryState.Undiscovered;
        }

        private static ContentCatalogDiscoveryState ResolveUnlockableDiscovery(
            UnlockableDef unlockable,
            UserProfile profile
        )
        {
            if (unlockable == null)
            {
                return
                    ContentCatalogDiscoveryState.NotApplicable;
            }

            return
                profile.HasUnlockable(
                    unlockable
                )
                    ? ContentCatalogDiscoveryState.Discovered
                    : ContentCatalogDiscoveryState.Undiscovered;
        }

        private static UserProfile TryGetLocalProfile()
        {
            try
            {
                if (
                    LocalUserManager.readOnlyLocalUsersList != null &&
                    LocalUserManager.readOnlyLocalUsersList.Count > 0
                )
                {
                    LocalUser localUser =
                        LocalUserManager.readOnlyLocalUsersList[0];

                    if (localUser != null)
                    {
                        return
                            localUser.userProfile;
                    }
                }
            }
            catch
            {
                // Perfil todavía no preparado. Se podrá refrescar más tarde.
            }

            return null;
        }
    }
}
