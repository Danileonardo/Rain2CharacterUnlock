using System;
using R2API;

namespace UniversalSurvivorUnlocks
{
    public static class SurvivorLocalizationKeys
    {
        public const string Sora = "official.sora";
        public const string Ralsei = "official.ralsei";
        public const string Jhin = "official.jhin";
        public const string Scout = "official.scout";
        public const string Spy = "official.spy";
        public const string Rocket = "official.rocket";
        public const string Hunk = "official.hunk";
        public const string Tinkaton = "official.tinkaton";
        public const string Wooper = "official.wooper";
    }


    /// <summary>
    /// Localización integrada de los presets oficiales de USU.
    /// RoR2 elige automáticamente el idioma actual al resolver los tokens.
    /// </summary>
    public static class SurvivorLocalization
    {
        public const string ExcludedSurvivorsNoticeToken =
            "USU_MISSION_NOTICE_EXCLUDED_SURVIVORS";

        public const string MissionLibraryButtonToken =
            "USU_LIBRARY_BUTTON";
        public const string MissionLibraryWindowTitleToken =
            "USU_LIBRARY_WINDOW_TITLE";
        public const string MissionLibraryHeadingToken =
            "USU_LIBRARY_HEADING";
        public const string MissionLibraryRefreshToken =
            "USU_LIBRARY_REFRESH";
        public const string MissionLibrarySurvivorsToken =
            "USU_LIBRARY_SURVIVORS";
        public const string MissionLibraryNoSurvivorsToken =
            "USU_LIBRARY_NO_SURVIVORS";
        public const string MissionLibraryUnavailableSuffixToken =
            "USU_LIBRARY_UNAVAILABLE_SUFFIX";
        public const string MissionLibrarySelectSurvivorToken =
            "USU_LIBRARY_SELECT_SURVIVOR";
        public const string MissionLibraryProviderToken =
            "USU_LIBRARY_PROVIDER";
        public const string MissionLibrarySelectionModeToken =
            "USU_LIBRARY_SELECTION_MODE";
        public const string MissionLibraryOriginalAvailableToken =
            "USU_LIBRARY_ORIGINAL_AVAILABLE";
        public const string MissionLibraryCurrentMissionToken =
            "USU_LIBRARY_CURRENT_MISSION";
        public const string MissionLibraryProvidersToken =
            "USU_LIBRARY_PROVIDERS";
        public const string MissionLibraryUseOriginalToken =
            "USU_LIBRARY_USE_ORIGINAL";
        public const string MissionLibraryRestoreToken =
            "USU_LIBRARY_RESTORE";
        public const string MissionLibraryCustomComingSoonToken =
            "USU_LIBRARY_CUSTOM_COMING_SOON";
        public const string MissionLibraryHostOnlyToken =
            "USU_LIBRARY_HOST_ONLY";
        public const string MissionLibraryPresetsToken =
            "USU_LIBRARY_PRESETS";
        public const string MissionLibraryPresetHintToken =
            "USU_LIBRARY_PRESET_HINT";
        public const string MissionLibraryNoPresetsToken =
            "USU_LIBRARY_NO_PRESETS";
        public const string MissionLibraryAssignToken =
            "USU_LIBRARY_ASSIGN";
        public const string MissionLibraryDesignedForToken =
            "USU_LIBRARY_DESIGNED_FOR";
        public const string MissionLibraryLastActionPresetToken =
            "USU_LIBRARY_LAST_ACTION_PRESET";
        public const string MissionLibraryFooterToken =
            "USU_LIBRARY_FOOTER";
        public const string MissionLibraryYesToken =
            "USU_LIBRARY_YES";
        public const string MissionLibraryNoToken =
            "USU_LIBRARY_NO";

        // 5G.2B - presentación de proveedor / misión activa.
        public const string MissionLibraryProviderSectionToken =
            "USU_LIBRARY_PROVIDER_SECTION";
        public const string MissionLibraryProviderOriginalToken =
            "USU_LIBRARY_PROVIDER_ORIGINAL";
        public const string MissionLibraryProviderUsuToken =
            "USU_LIBRARY_PROVIDER_USU";
        public const string MissionLibraryProviderCommunityToken =
            "USU_LIBRARY_PROVIDER_COMMUNITY";
        public const string MissionLibraryProviderCustomToken =
            "USU_LIBRARY_PROVIDER_CUSTOM";
        public const string MissionLibrarySoonSuffixToken =
            "USU_LIBRARY_SOON_SUFFIX";
        public const string MissionLibraryOriginalDescriptionToken =
            "USU_LIBRARY_ORIGINAL_DESCRIPTION";
        public const string MissionLibraryUsuDescriptionToken =
            "USU_LIBRARY_USU_DESCRIPTION";
        public const string MissionLibraryCustomDescriptionToken =
            "USU_LIBRARY_CUSTOM_DESCRIPTION";
        public const string MissionLibraryChoosePresetForUsuToken =
            "USU_LIBRARY_CHOOSE_PRESET_FOR_USU";
        public const string MissionLibraryChoosePresetActionToken =
            "USU_LIBRARY_CHOOSE_PRESET_ACTION";
        public const string MissionLibraryOriginalUnavailableToken =
            "USU_LIBRARY_ORIGINAL_UNAVAILABLE";
        public const string MissionLibraryActiveMissionToken =
            "USU_LIBRARY_ACTIVE_MISSION";
        public const string MissionLibraryOriginalMissionTitleToken =
            "USU_LIBRARY_ORIGINAL_MISSION_TITLE";
        public const string MissionLibraryOriginalMissionDescriptionToken =
            "USU_LIBRARY_ORIGINAL_MISSION_DESCRIPTION";
        public const string MissionLibraryNoActiveMissionToken =
            "USU_LIBRARY_NO_ACTIVE_MISSION";
        public const string MissionLibraryRestoreHintToken =
            "USU_LIBRARY_RESTORE_HINT";
        public const string MissionLibraryInUseToken =
            "USU_LIBRARY_IN_USE";
        public const string MissionLibraryUseThisMissionToken =
            "USU_LIBRARY_USE_THIS_MISSION";
        public const string MissionLibraryNotValidWithToken =
            "USU_LIBRARY_NOT_VALID_WITH";

        // 5G.2C FIX1 - política Original / USU / Custom.
        public const string MissionLibraryUsuUnavailableWhenOriginalToken =
            "USU_LIBRARY_USU_UNAVAILABLE_WHEN_ORIGINAL";
        public const string MissionLibraryCustomFromOriginalSoonToken =
            "USU_LIBRARY_CUSTOM_FROM_ORIGINAL_SOON";
        public const string MissionLibraryPresetHintOriginalToken =
            "USU_LIBRARY_PRESET_HINT_ORIGINAL";
        public const string MissionLibraryPresetCustomSoonToken =
            "USU_LIBRARY_PRESET_CUSTOM_SOON";

        // 5G.2C - entrada desde Settings / Risk of Options.
        public const string MissionLibrarySettingsOptionNameToken =
            "USU_LIBRARY_SETTINGS_OPTION_NAME";
        public const string MissionLibrarySettingsCategoryToken =
            "USU_LIBRARY_SETTINGS_CATEGORY";
        public const string MissionLibrarySettingsDescriptionToken =
            "USU_LIBRARY_SETTINGS_DESCRIPTION";
        public const string MissionLibrarySettingsOpenToken =
            "USU_LIBRARY_SETTINGS_OPEN";
        public const string MissionLibrarySettingsModDescriptionToken =
            "USU_LIBRARY_SETTINGS_MOD_DESCRIPTION";

        // 5G.2E-A - shell embebido dentro de Risk of Options.
        public const string MissionLibraryEmbeddedGeneralCategoryToken =
            "USU_EMBED_CATEGORY_GENERAL";
        public const string MissionLibraryEmbeddedSurvivorsCategoryToken =
            "USU_EMBED_CATEGORY_SURVIVORS";
        public const string MissionLibraryEmbeddedMissionsCategoryToken =
            "USU_EMBED_CATEGORY_MISSIONS";
        public const string MissionLibraryEmbeddedAdvancedCategoryToken =
            "USU_EMBED_CATEGORY_ADVANCED";
        public const string MissionLibraryEmbeddedMarkerDescriptionToken =
            "USU_EMBED_MARKER_DESCRIPTION";

        public const string MissionLibraryEmbeddedTitleToken =
            "USU_EMBED_TITLE";
        public const string MissionLibraryEmbeddedIntegratedToken =
            "USU_EMBED_INTEGRATED";
        public const string MissionLibraryEmbeddedInterfaceToken =
            "USU_EMBED_INTERFACE";
        public const string MissionLibraryEmbeddedDetectedSurvivorsToken =
            "USU_EMBED_DETECTED_SURVIVORS";
        public const string MissionLibraryEmbeddedLinkedSkillsToken =
            "USU_EMBED_LINKED_SKILLS";
        public const string MissionLibraryEmbeddedDetectedSkinsToken =
            "USU_EMBED_DETECTED_SKINS";
        public const string MissionLibraryEmbeddedOriginalToken =
            "USU_EMBED_ORIGINAL";
        public const string MissionLibraryEmbeddedUsuToken =
            "USU_EMBED_USU";
        public const string MissionLibraryEmbeddedFreeToken =
            "USU_EMBED_FREE";
        public const string MissionLibraryEmbeddedExternalWindowToken =
            "USU_EMBED_EXTERNAL_WINDOW";
        public const string MissionLibraryEmbeddedDisabledToken =
            "USU_EMBED_DISABLED";

        public const string MissionLibraryEmbeddedProfilesToken =
            "USU_EMBED_PROFILES";
        public const string MissionLibraryEmbeddedLoreToken =
            "USU_EMBED_LORE";
        public const string MissionLibraryEmbeddedSkillGroupsToken =
            "USU_EMBED_SKILL_GROUPS";
        public const string MissionLibraryEmbeddedSkillVariantsToken =
            "USU_EMBED_SKILL_VARIANTS";
        public const string MissionLibraryEmbeddedLockedSkillsToken =
            "USU_EMBED_LOCKED_SKILLS";
        public const string MissionLibraryEmbeddedSkinsToken =
            "USU_EMBED_SKINS";
        public const string MissionLibraryEmbeddedLockedSkinsToken =
            "USU_EMBED_LOCKED_SKINS";
        public const string MissionLibraryEmbeddedVisualBrowserToken =
            "USU_EMBED_VISUAL_BROWSER";
        public const string MissionLibraryEmbeddedNextBrowserToken =
            "USU_EMBED_NEXT_BROWSER";

        public const string MissionLibraryEmbeddedAssignablePresetsToken =
            "USU_EMBED_ASSIGNABLE_PRESETS";
        public const string MissionLibraryEmbeddedProvidersToken =
            "USU_EMBED_PROVIDERS";
        public const string MissionLibraryEmbeddedLibraryToken =
            "USU_EMBED_LIBRARY";
        public const string MissionLibraryEmbeddedCustomToken =
            "USU_EMBED_CUSTOM";
        public const string MissionLibraryEmbeddedEditorToken =
            "USU_EMBED_EDITOR";
        public const string MissionLibraryEmbeddedSoon5HToken =
            "USU_EMBED_SOON_5H";
        public const string MissionLibraryEmbeddedSoon5IToken =
            "USU_EMBED_SOON_5I";

        public const string MissionLibraryEmbeddedRuntimeCatalogToken =
            "USU_EMBED_RUNTIME_CATALOG";
        public const string MissionLibraryEmbeddedItemsToken =
            "USU_EMBED_ITEMS";
        public const string MissionLibraryEmbeddedEquipmentToken =
            "USU_EMBED_EQUIPMENT";
        public const string MissionLibraryEmbeddedEnemiesToken =
            "USU_EMBED_ENEMIES";
        public const string MissionLibraryEmbeddedBossesToken =
            "USU_EMBED_BOSSES";
        public const string MissionLibraryEmbeddedStagesToken =
            "USU_EMBED_STAGES";
        public const string MissionLibraryEmbeddedDiscoveredToken =
            "USU_EMBED_DISCOVERED";
        public const string MissionLibraryEmbeddedUndiscoveredToken =
            "USU_EMBED_UNDISCOVERED";
        public const string MissionLibraryEmbeddedUnknownDiscoveryToken =
            "USU_EMBED_UNKNOWN_DISCOVERY";

        // 5G.2E-B - navegador visual de personajes embebido.
        public const string MissionLibraryBrowserViewToken =
            "USU_BROWSER_VIEW";
        public const string MissionLibraryBrowserStatusToken =
            "USU_BROWSER_STATUS";
        public const string MissionLibraryBrowserSourceToken =
            "USU_BROWSER_SOURCE";
        public const string MissionLibraryBrowserUnlockMissionToken =
            "USU_BROWSER_UNLOCK_MISSION";
        public const string MissionLibraryBrowserLockedToken =
            "USU_BROWSER_LOCKED";
        public const string MissionLibraryBrowserUnlockedToken =
            "USU_BROWSER_UNLOCKED";
        public const string MissionLibraryBrowserUnknownToken =
            "USU_BROWSER_UNKNOWN";
        public const string MissionLibraryBrowserUnlockedByDefaultToken =
            "USU_BROWSER_UNLOCKED_BY_DEFAULT";
        public const string MissionLibraryBrowserNoMissionRequiredToken =
            "USU_BROWSER_NO_MISSION_REQUIRED";
        public const string MissionLibraryBrowserDescriptionToken =
            "USU_BROWSER_DESCRIPTION";
        public const string MissionLibraryBrowserNotesToken =
            "USU_BROWSER_NOTES";
        public const string MissionLibraryBrowserNotesLockedToken =
            "USU_BROWSER_NOTES_LOCKED";
        public const string MissionLibraryBrowserNotesUnavailableToken =
            "USU_BROWSER_NOTES_UNAVAILABLE";
        public const string MissionLibraryBrowserNotesFallbackDemoToken =
            "USU_BROWSER_NOTES_FALLBACK_DEMO";


        private static bool initialized;


        private sealed class OfficialText
        {
            public string BodyName;
            public string LocalizationKey;
            public string SpanishName;
            public string SpanishDescription;
            public string EnglishName;
            public string EnglishDescription;
        }


        private static readonly OfficialText[] OfficialTexts =
        {
            new OfficialText
            {
                BodyName = "SoraBody",
                LocalizationKey = SurvivorLocalizationKeys.Sora,
                SpanishName = "Elegido de la Llave Espada",
                SpanishDescription =
                    "Abre paso entre mundos en Baluarte de Ambry;\n" +
                    "vence a sombras y completa Venganza - Mercenary",
                EnglishName = "Chosen of the Keyblade",
                EnglishDescription =
                    "Cross worlds through Bulwark's Ambry;\n" +
                    "defeat shadows; complete Vengeance - Mercenary"
            },

            new OfficialText
            {
                BodyName = "RalseiBody",
                LocalizationKey = SurvivorLocalizationKeys.Ralsei,
                SpanishName = "El poder de la bondad",
                SpanishDescription =
                    "Usa Devoción y reúne 3 nuevos amigos Lemurianos;\n" +
                    "completa el portal con ellos - Captain o Seeker",
                EnglishName = "The Power of Kindness",
                EnglishDescription =
                    "Use Devotion; recruit 3 Lemurian friends;\n" +
                    "complete the teleporter - Captain or Seeker"
            },

            new OfficialText
            {
                BodyName = "JhinBody",
                LocalizationKey = SurvivorLocalizationKeys.Jhin,
                SpanishName = "El Cuarto Acto",
                SpanishDescription =
                    "Convierte a un jefe en tu gran final;\n" +
                    "asesta un crítico mortal de 4.444 de daño o más.",
                EnglishName = "The Fourth Act",
                EnglishDescription =
                    "Turn a boss into your grand finale;\n" +
                    "land a lethal critical hit for 4,444+ damage."
            },

            new OfficialText
            {
                BodyName = "ScoutBody",
                LocalizationKey = SurvivorLocalizationKeys.Scout,
                SpanishName = "Sed Termonuclear",
                SpanishDescription =
                    "Sacia tu sed con 8 Bebidas energéticas;\n" +
                    "o completa el primer sector sin objetos en 4 min.",
                EnglishName = "Thermonuclear Thirst",
                EnglishDescription =
                    "Quench your thirst with 8 Energy Drinks;\n" +
                    "or clear the first stage itemless in 4 min."
            },

            new OfficialText
            {
                BodyName = "SpyBody",
                LocalizationKey = SurvivorLocalizationKeys.Spy,
                SpanishName = "Sin que me veas venir",
                SpanishDescription =
                    "Que el jefe nunca vea venir tu golpe final;\n" +
                    "remátalo por detrás con Daga serrada - Bandit",
                EnglishName = "Never Saw Me Coming",
                EnglishDescription =
                    "Make sure the boss never sees the final blow;\n" +
                    "backstab the boss with Serrated Dagger - Bandit"
            },

            new OfficialText
            {
                BodyName = "RocketSurvivorBody",
                LocalizationKey = SurvivorLocalizationKeys.Rocket,
                SpanishName = "La gravedad es opcional",
                SpanishDescription =
                    "Haz llover explosiones desde el cielo;\n" +
                    "derriba 5 antes de caer; haz la hazaña 3 veces.",
                EnglishName = "Gravity Is Optional",
                EnglishDescription =
                    "Rain explosions down from the sky;\n" +
                    "drop 5 before landing; pull it off 3 times."
            },

            new OfficialText
            {
                BodyName = "RobHunkBody",
                LocalizationKey = SurvivorLocalizationKeys.Hunk,
                SpanishName = "La Parca No Falla",
                SpanishDescription =
                    "Protege la batería y sobrevive a toda costa;\n" +
                    "escapa de la Luna o sacrifícate en el Obelisco.",
                EnglishName = "The Reaper Never Fails",
                EnglishDescription =
                    "Protect the Fuel Array; survive at all costs;\n" +
                    "escape the Moon or sacrifice at the Obelisk."
            },

            new OfficialText
            {
                BodyName = "TinkatonBody",
                LocalizationKey = SurvivorLocalizationKeys.Tinkaton,
                SpanishName = "Forjada en Chatarra",
                SpanishDescription =
                    "Haz de 6 chatarras el inicio de tu gran golpe;\n" +
                    "ten Justicia demoledora y vence un Ojo mecánico.",
                EnglishName = "Forged in Scrap",
                EnglishDescription =
                    "Scrap 6 items to prepare your crushing blow;\n" +
                    "hold Shattering Justice; defeat a mech Eye."
            },

            new OfficialText
            {
                BodyName = "WooperBody",
                LocalizationKey = SurvivorLocalizationKeys.Wooper,
                SpanishName = "De vuelta al agua",
                SpanishDescription =
                    "Haz de los Humedales tu hogar; marca territorio;\n" +
                    "caza y muerde a 20 presas envenenadas - Acrid",
                EnglishName = "Back to the Water",
                EnglishDescription =
                    "Make the Wetlands your home; mark your territory;\n" +
                    "hunt and bite 20 poisoned prey - Acrid"
            }
        };


        public static void EnsureRegistered()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            RegisterSystemTexts();

            for (int i = 0; i < OfficialTexts.Length; i++)
            {
                RegisterOfficialText(OfficialTexts[i]);
            }
        }


        public static bool UsesBuiltInLocalization(
            string bodyName,
            SurvivorChallengeJson challenge
        )
        {
            if (
                string.IsNullOrWhiteSpace(bodyName) ||
                challenge == null ||
                string.IsNullOrWhiteSpace(challenge.LocalizationKey)
            )
            {
                return false;
            }

            for (int i = 0; i < OfficialTexts.Length; i++)
            {
                OfficialText text = OfficialTexts[i];

                if (
                    string.Equals(
                        text.BodyName,
                        bodyName,
                        StringComparison.Ordinal
                    ) &&
                    string.Equals(
                        text.LocalizationKey,
                        challenge.LocalizationKey,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return true;
                }
            }

            return false;
        }


        public static bool TryGetOfficialMissionTokens(
            string bodyName,
            out string nameToken,
            out string descriptionToken
        )
        {
            nameToken = "";
            descriptionToken = "";

            if (string.IsNullOrWhiteSpace(bodyName))
            {
                return false;
            }

            for (int i = 0; i < OfficialTexts.Length; i++)
            {
                OfficialText text = OfficialTexts[i];

                if (
                    text != null &&
                    string.Equals(
                        text.BodyName,
                        bodyName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    string tokenPrefix =
                        GetOfficialTokenPrefix(text.BodyName);

                    nameToken =
                        $"{tokenPrefix}_ACHIEVEMENT_NAME";

                    descriptionToken =
                        $"{tokenPrefix}_ACHIEVEMENT_DESCRIPTION";

                    return true;
                }
            }

            return false;
        }


        public static string GetOfficialTokenPrefix(string bodyName)
        {
            return $"USU_{MakeToken(bodyName)}_OFFICIAL";
        }


        public static string GetCustomTokenPrefix(string bodyName)
        {
            return $"USU_{MakeToken(bodyName)}_CUSTOM";
        }


        private static void RegisterSystemTexts()
        {
            const string english =
                "<style=cIsUtility>Notice:</style> Not valid with: {0}";

            const string spanish =
                "<style=cIsUtility>Aviso:</style> No válido con: {0}";


            // Inglés = fallback para idiomas aún no traducidos.
            LanguageAPI.Add(
                ExcludedSurvivorsNoticeToken,
                english
            );

            LanguageAPI.Add(
                ExcludedSurvivorsNoticeToken,
                english,
                "en"
            );

            LanguageAPI.Add(
                ExcludedSurvivorsNoticeToken,
                spanish,
                "es-419"
            );

            LanguageAPI.Add(
                ExcludedSurvivorsNoticeToken,
                spanish,
                "es-ES"
            );

            RegisterUiText(MissionLibraryButtonToken, "USU", "USU");
            RegisterUiText(MissionLibraryWindowTitleToken, "Universal Survivor Unlocks", "Universal Survivor Unlocks");
            RegisterUiText(MissionLibraryHeadingToken, "Mission Library", "Biblioteca de misiones");
            RegisterUiText(MissionLibraryRefreshToken, "Refresh", "Actualizar");
            RegisterUiText(MissionLibrarySurvivorsToken, "Survivors", "Personajes");
            RegisterUiText(MissionLibraryNoSurvivorsToken, "No survivors detected.", "No hay personajes detectados.");
            RegisterUiText(MissionLibraryUnavailableSuffixToken, "[Unavailable]", "[No disponible]");
            RegisterUiText(MissionLibrarySelectSurvivorToken, "Select a survivor to configure it.", "Selecciona un personaje para configurarlo.");
            RegisterUiText(MissionLibraryProviderToken, "Provider: {0}", "Proveedor: {0}");
            RegisterUiText(MissionLibrarySelectionModeToken, "Selection: {0}", "Selección: {0}");
            RegisterUiText(MissionLibraryOriginalAvailableToken, "Original available: {0}", "Original disponible: {0}");
            RegisterUiText(MissionLibraryCurrentMissionToken, "Current USU mission", "Misión USU actual");
            RegisterUiText(MissionLibraryProvidersToken, "Provider actions", "Acciones de proveedor");
            RegisterUiText(MissionLibraryUseOriginalToken, "Use Original", "Usar Original");
            RegisterUiText(MissionLibraryRestoreToken, "Restore character", "Restaurar personaje");
            RegisterUiText(MissionLibraryCustomComingSoonToken, "Custom missions will be exposed in 5H/5I.", "Las misiones Custom se habilitarán en 5H/5I.");
            RegisterUiText(MissionLibraryHostOnlyToken, "Only the host can change mission providers in multiplayer.", "Sólo el host puede cambiar proveedores de misión en multijugador.");
            RegisterUiText(MissionLibraryPresetsToken, "USU mission presets", "Presets de misión USU");
            RegisterUiText(MissionLibraryPresetHintToken, "Any assignable preset can be applied to any detected survivor.", "Cualquier preset asignable puede aplicarse a cualquier personaje detectado.");
            RegisterUiText(MissionLibraryNoPresetsToken, "No assignable presets are available.", "No hay presets asignables disponibles.");
            RegisterUiText(MissionLibraryAssignToken, "Assign", "Asignar");
            RegisterUiText(MissionLibraryDesignedForToken, "Designed for: {0}", "Diseñado para: {0}");
            RegisterUiText(MissionLibraryLastActionPresetToken, "Last preset selected in this panel.", "Último preset seleccionado en este panel.");
            RegisterUiText(MissionLibraryFooterToken, "Changes are saved immediately and applied at runtime when possible.", "Los cambios se guardan inmediatamente y se aplican en runtime cuando es posible.");
            RegisterUiText(MissionLibraryYesToken, "Yes", "Sí");
            RegisterUiText(MissionLibraryNoToken, "No", "No");

            // 5G.2B - textos de la presentación final básica de la biblioteca.
            RegisterUiText(MissionLibraryProviderSectionToken, "Unlock provider", "Proveedor de desbloqueo");
            RegisterUiText(MissionLibraryProviderOriginalToken, "Original", "Original");
            RegisterUiText(MissionLibraryProviderUsuToken, "USU", "USU");
            RegisterUiText(MissionLibraryProviderCommunityToken, "Community", "Comunidad");
            RegisterUiText(MissionLibraryProviderCustomToken, "Custom", "Personalizado");
            RegisterUiText(MissionLibrarySoonSuffixToken, "[Soon]", "[Próximamente]");
            RegisterUiText(MissionLibraryOriginalDescriptionToken, "The character creator's original unlock system is in control.", "El sistema de desbloqueo original del creador está en control.");
            RegisterUiText(MissionLibraryUsuDescriptionToken, "Universal Survivor Unlocks controls this character through the selected mission.", "Universal Survivor Unlocks controla este personaje mediante la misión seleccionada.");
            RegisterUiText(MissionLibraryCustomDescriptionToken, "A player-created mission controls this character.", "Una misión creada por el jugador controla este personaje.");
            RegisterUiText(MissionLibraryChoosePresetForUsuToken, "Choose a mission below to activate USU for this character.", "Elige una misión de la biblioteca para activar USU en este personaje.");
            RegisterUiText(MissionLibraryChoosePresetActionToken, "Choose a mission below. Using it will switch this character to USU automatically.", "Elige una misión de abajo. Al usarla, este personaje cambiará a USU automáticamente.");
            RegisterUiText(MissionLibraryOriginalUnavailableToken, "Original is not available for this character; USU acts as fallback.", "Original no está disponible para este personaje; USU actúa como fallback.");
            RegisterUiText(MissionLibraryActiveMissionToken, "Active unlock", "Desbloqueo activo");
            RegisterUiText(MissionLibraryOriginalMissionTitleToken, "Original system", "Sistema original");
            RegisterUiText(MissionLibraryOriginalMissionDescriptionToken, "USU leaves the unlock rules exactly as defined by the character creator.", "USU deja las reglas de desbloqueo exactamente como las definió el creador del personaje.");
            RegisterUiText(MissionLibraryNoActiveMissionToken, "No active mission information is available.", "No hay información disponible de la misión activa.");
            RegisterUiText(MissionLibraryRestoreHintToken, "Restore removes this character's player CUSTOM copies and returns to its original/fallback behavior.", "Restaurar elimina las copias CUSTOM del jugador para este personaje y vuelve a su comportamiento original/fallback.");
            RegisterUiText(MissionLibraryInUseToken, "In use", "En uso");
            RegisterUiText(MissionLibraryUseThisMissionToken, "Use this mission", "Usar esta misión");
            RegisterUiText(MissionLibraryNotValidWithToken, "Notice: Not valid with: {0}", "Aviso: No válido con: {0}");

            // 5G.2C - Risk of Options / Mod Options (tokens legacy/fallback).
            RegisterUiText(MissionLibrarySettingsOptionNameToken, "Mission Library", "Biblioteca de misiones");
            RegisterUiText(MissionLibrarySettingsCategoryToken, "Unlocks", "Desbloqueos");
            RegisterUiText(MissionLibrarySettingsDescriptionToken, "Open the Universal Survivor Unlocks mission and provider manager.", "Abre el administrador de misiones y proveedores de Universal Survivor Unlocks.");
            RegisterUiText(MissionLibrarySettingsOpenToken, "Open", "Abrir");
            RegisterUiText(MissionLibrarySettingsModDescriptionToken,
                "Manage survivors, unlock missions, providers and future custom content directly inside Risk of Options.",
                "Administra personajes, misiones de desbloqueo, proveedores y futuro contenido CUSTOM directamente dentro de Risk of Options.");

            // 5G.2E-A - categorías y datos embebidos.
            RegisterUiText(MissionLibraryEmbeddedGeneralCategoryToken, "General", "General");
            RegisterUiText(MissionLibraryEmbeddedSurvivorsCategoryToken, "Survivors", "Personajes");
            RegisterUiText(MissionLibraryEmbeddedMissionsCategoryToken, "Missions", "Misiones");
            RegisterUiText(MissionLibraryEmbeddedAdvancedCategoryToken, "Advanced", "Avanzado");
            RegisterUiText(MissionLibraryEmbeddedMarkerDescriptionToken, "USU embedded content anchor.", "Ancla de contenido embebido de USU.");

            RegisterUiText(MissionLibraryEmbeddedTitleToken, "Universal Survivor Unlocks", "Universal Survivor Unlocks");
            RegisterUiText(MissionLibraryEmbeddedIntegratedToken, "Integrated", "Integrada");
            RegisterUiText(MissionLibraryEmbeddedInterfaceToken, "Interface", "Interfaz");
            RegisterUiText(MissionLibraryEmbeddedDetectedSurvivorsToken, "Detected survivors", "Personajes detectados");
            RegisterUiText(MissionLibraryEmbeddedLinkedSkillsToken, "Linked skill variants", "Variantes de habilidad vinculadas");
            RegisterUiText(MissionLibraryEmbeddedDetectedSkinsToken, "Detected skins", "Skins detectadas");
            RegisterUiText(MissionLibraryEmbeddedOriginalToken, "Original", "Original");
            RegisterUiText(MissionLibraryEmbeddedUsuToken, "USU managed", "Administrados por USU");
            RegisterUiText(MissionLibraryEmbeddedFreeToken, "Unlocked by default", "Libres desde el inicio");
            RegisterUiText(MissionLibraryEmbeddedExternalWindowToken, "External window", "Ventana externa");
            RegisterUiText(MissionLibraryEmbeddedDisabledToken, "Disabled", "Desactivada");

            RegisterUiText(MissionLibraryEmbeddedProfilesToken, "Survivor profiles", "Perfiles de personaje");
            RegisterUiText(MissionLibraryEmbeddedLoreToken, "Lore resolved", "Lore resuelto");
            RegisterUiText(MissionLibraryEmbeddedSkillGroupsToken, "Skill groups", "Grupos de habilidad");
            RegisterUiText(MissionLibraryEmbeddedSkillVariantsToken, "Skill variants", "Variantes de habilidad");
            RegisterUiText(MissionLibraryEmbeddedLockedSkillsToken, "Locked skills", "Skills bloqueadas");
            RegisterUiText(MissionLibraryEmbeddedSkinsToken, "Skins", "Skins");
            RegisterUiText(MissionLibraryEmbeddedLockedSkinsToken, "Locked skins", "Skins bloqueadas");
            RegisterUiText(MissionLibraryEmbeddedVisualBrowserToken, "Visual survivor browser", "Navegador visual de personajes");
            RegisterUiText(MissionLibraryEmbeddedNextBrowserToken, "Next: 5G.2E-B", "Siguiente: 5G.2E-B");

            RegisterUiText(MissionLibraryEmbeddedAssignablePresetsToken, "Assignable presets", "Presets asignables");
            RegisterUiText(MissionLibraryEmbeddedProvidersToken, "Providers", "Proveedores");
            RegisterUiText(MissionLibraryEmbeddedLibraryToken, "Mission library", "Biblioteca de misiones");
            RegisterUiText(MissionLibraryEmbeddedCustomToken, "Custom missions", "Misiones personalizadas");
            RegisterUiText(MissionLibraryEmbeddedEditorToken, "Mission editor", "Editor de misiones");
            RegisterUiText(MissionLibraryEmbeddedSoon5HToken, "Coming in 5H", "Próximamente en 5H");
            RegisterUiText(MissionLibraryEmbeddedSoon5IToken, "Coming in 5I", "Próximamente en 5I");

            RegisterUiText(MissionLibraryEmbeddedRuntimeCatalogToken, "Runtime catalog", "Catálogo runtime");
            RegisterUiText(MissionLibraryEmbeddedItemsToken, "Items", "Objetos");
            RegisterUiText(MissionLibraryEmbeddedEquipmentToken, "Equipment", "Equipamientos");
            RegisterUiText(MissionLibraryEmbeddedEnemiesToken, "Enemies", "Enemigos");
            RegisterUiText(MissionLibraryEmbeddedBossesToken, "Bosses / Champions", "Jefes / Champions");
            RegisterUiText(MissionLibraryEmbeddedStagesToken, "Stages", "Escenarios");
            RegisterUiText(MissionLibraryEmbeddedDiscoveredToken, "Discovered", "Descubiertos");
            RegisterUiText(MissionLibraryEmbeddedUndiscoveredToken, "Undiscovered", "No descubiertos");
            RegisterUiText(MissionLibraryEmbeddedUnknownDiscoveryToken, "Unknown discovery state", "Estado de descubrimiento desconocido");

            // 5G.2E-B - navegador visual de personajes.
            RegisterUiText(MissionLibraryBrowserViewToken, "View", "Ver");
            RegisterUiText(MissionLibraryBrowserStatusToken, "Status", "Estado");
            RegisterUiText(MissionLibraryBrowserSourceToken, "Source", "Fuente");
            RegisterUiText(MissionLibraryBrowserUnlockMissionToken, "Unlock mission", "Misión de desbloqueo");
            RegisterUiText(MissionLibraryBrowserLockedToken, "Locked", "Bloqueado");
            RegisterUiText(MissionLibraryBrowserUnlockedToken, "Unlocked", "Desbloqueado");
            RegisterUiText(MissionLibraryBrowserUnknownToken, "Unknown", "Desconocido");
            RegisterUiText(MissionLibraryBrowserUnlockedByDefaultToken, "Unlocked from the start", "Disponible desde el inicio");
            RegisterUiText(MissionLibraryBrowserNoMissionRequiredToken, "The creator does not require an unlock mission for this content.", "El creador no exige una misión de desbloqueo para este contenido.");
            RegisterUiText(MissionLibraryBrowserDescriptionToken, "Description", "Descripción");
            RegisterUiText(MissionLibraryBrowserNotesToken, "Notes", "Notas");
            RegisterUiText(MissionLibraryBrowserNotesLockedToken, "Logbook entry not discovered.", "Entrada de diario no descubierta.");
            RegisterUiText(MissionLibraryBrowserNotesUnavailableToken, "No Logbook notes are available for this survivor.", "No hay notas de Diario disponibles para este personaje.");
            RegisterUiText(MissionLibraryBrowserNotesFallbackDemoToken, "[USU fallback demo] This survivor does not expose a native Logbook entry. A localized fallback can be supplied here without modifying the creator's content.", "[Demo fallback USU] Este personaje no expone una entrada nativa de Diario. Aquí se podrá proporcionar un fallback localizado sin modificar el contenido del creador.");
        }


        private static void RegisterUiText(
            string token,
            string english,
            string spanish
        )
        {
            LanguageAPI.Add(token, english);
            LanguageAPI.Add(token, english, "en");
            LanguageAPI.Add(token, spanish, "es-419");
            LanguageAPI.Add(token, spanish, "es-ES");
        }


        private static void RegisterOfficialText(OfficialText text)
        {
            if (text == null)
            {
                return;
            }

            string tokenPrefix =
                GetOfficialTokenPrefix(text.BodyName);

            string nameToken =
                $"{tokenPrefix}_ACHIEVEMENT_NAME";

            string descriptionToken =
                $"{tokenPrefix}_ACHIEVEMENT_DESCRIPTION";

            // Inglés = fallback para cualquier idioma todavía no traducido.
            LanguageAPI.Add(nameToken, text.EnglishName);
            LanguageAPI.Add(descriptionToken, text.EnglishDescription);

            LanguageAPI.Add(nameToken, text.EnglishName, "en");
            LanguageAPI.Add(descriptionToken, text.EnglishDescription, "en");

            RegisterSpanish(nameToken, text.SpanishName);
            RegisterSpanish(descriptionToken, text.SpanishDescription);
        }


        private static void RegisterSpanish(
            string token,
            string value
        )
        {
            LanguageAPI.Add(token, value, "es-419");
            LanguageAPI.Add(token, value, "es-ES");
        }


        private static string MakeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "UNKNOWN";
            }

            return value
                .Replace(".", "_")
                .Replace("-", "_")
                .Replace(" ", "_")
                .ToUpperInvariant();
        }
    }
}
