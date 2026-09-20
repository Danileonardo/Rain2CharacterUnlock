using System;
using System.Collections.Generic;

using BepInEx.Logging;

using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public static partial class SurvivorUnlockManager
    {
        private static ManualLogSource
            effectivePresentationLogger;

        private static bool
            effectivePresentationInitialized;


        /// <summary>
        /// G8 Phase 1.3.
        ///
        /// Los AchievementDef de USU existen antes de G7/G8. Para que los
        /// popups vanilla (incluido unlock/relock por mods externos) no se
        /// queden congelados en el histórico KillEnemies 100, cada achievement
        /// de USU usa tokens EFFECTIVE y esos tokens se refrescan desde la
        /// misión efectiva actual.
        ///
        /// El snapshot del host sólo vive en memoria. Nunca se escribe en la
        /// configuración local del cliente.
        /// </summary>
        public static void InitializeEffectiveMissionPresentation(
            ManualLogSource log
        )
        {
            effectivePresentationLogger =
                log;

            if (!effectivePresentationInitialized)
            {
                effectivePresentationInitialized =
                    true;

                Language.onCurrentLanguageChanged +=
                    OnEffectivePresentationLanguageChanged;

                SessionMissionRegistry.EffectiveSnapshotChanged +=
                    OnEffectivePresentationSnapshotChanged;
            }

            RefreshAllEffectiveMissionPresentationTokens(
                "G8 init"
            );
        }


        public static void RefreshEffectiveMissionPresentationToken(
            string bodyName,
            string reason = ""
        )
        {
            string normalizedBody =
                bodyName?.Trim() ?? "";

            if (
                string.IsNullOrWhiteSpace(normalizedBody) ||
                !CustomUnlockables.ContainsKey(normalizedBody)
            )
            {
                return;
            }

            SurvivorJsonEntry entry =
                GetEntry(normalizedBody);

            if (entry == null)
            {
                return;
            }

            /*
             * G8 Phase 1.4.2:
             * si el provider efectivo es Original, la presentación pertenece
             * al juego/mod creador. USU mantiene preparado su unlock dormido
             * para un posible cambio de provider, pero NO refresca ni intenta
             * representar esa misión mediante sus tokens EFFECTIVE.
             *
             * Esto también cubre AutomaticFallback cuando existe Original y
             * Miner cuando el jugador tiene seleccionado Original. Si Miner
             * cambia explícitamente a USU, el provider efectivo deja de ser
             * Original y la presentación USU vuelve a actualizarse.
             */
            bool originalAvailable =
                UnlockProviderPolicy.HasStoredOriginalUnlock(
                    entry
                );

            UnlockProviderKind effectiveProvider =
                UnlockProviderPolicy.ResolveEffectiveProvider(
                    entry,
                    originalAvailable
                );

            if (effectiveProvider == UnlockProviderKind.Original)
            {
                UsuLog.Verbose(
                    effectivePresentationLogger,
                    "[MISSION PRESENTATION] Original preservado | " +
                    $"Body: {normalizedBody} | " +
                    "Tokens EFFECTIVE de USU sin refrescar" +
                    FormatPresentationReason(reason)
                );

                return;
            }

            string nameToken =
                GetEffectiveAchievementNameToken(
                    normalizedBody
                );

            string descriptionToken =
                GetEffectiveAchievementDescriptionToken(
                    normalizedBody
                );

            string displayName =
                ResolveChallengeDisplayName(
                    normalizedBody,
                    entry,
                    nameToken
                );

            string displayDescription =
                ResolveChallengeDisplayDescription(
                    normalizedBody,
                    entry,
                    descriptionToken
                );

            RegisterCurrentLanguageValue(
                nameToken,
                displayName
            );

            RegisterCurrentLanguageValue(
                descriptionToken,
                displayDescription
            );

            UsuLog.Verbose(
                effectivePresentationLogger,
                "[MISSION PRESENTATION] Achievement efectivo actualizado | " +
                $"Body: {normalizedBody} | " +
                $"Name: {displayName} | " +
                $"Description: {EscapeForLog(displayDescription)}" +
                FormatPresentationReason(reason)
            );
        }


        public static void RefreshAllEffectiveMissionPresentationTokens(
            string reason = ""
        )
        {
            List<string> bodies =
                new List<string>(
                    CustomUnlockables.Keys
                );

            bodies.Sort(
                StringComparer.OrdinalIgnoreCase
            );

            for (int i = 0; i < bodies.Count; i++)
            {
                RefreshEffectiveMissionPresentationToken(
                    bodies[i],
                    reason
                );
            }
        }


        private static void OnEffectivePresentationLanguageChanged()
        {
            RefreshAllEffectiveMissionPresentationTokens(
                "language changed"
            );
        }


        private static void OnEffectivePresentationSnapshotChanged()
        {
            /*
             * En Character Select / Run, GetEntry() resuelve primero el
             * snapshot del host. Al desaparecer el snapshot, vuelve a la
             * configuración local automáticamente.
             */
            RefreshAllEffectiveMissionPresentationTokens(
                "session snapshot changed"
            );
        }


        private static void RegisterCurrentLanguageValue(
            string token,
            string value
        )
        {
            if (
                string.IsNullOrWhiteSpace(token) ||
                value == null
            )
            {
                return;
            }

            string language =
                Language.currentLanguageName
                ?? "";

            /*
             * El fallback permite que el token siempre exista. La variante
             * del idioma actual evita que el texto de un host en otro idioma
             * se convierta en presentación persistente del cliente.
             */
            LanguageAPI.Add(
                token,
                value
            );

            if (!string.IsNullOrWhiteSpace(language))
            {
                LanguageAPI.Add(
                    token,
                    value,
                    language
                );
            }
        }


        private static string GetEffectiveAchievementNameToken(
            string bodyName
        )
        {
            return
                $"USU_{MakeToken(bodyName)}_EFFECTIVE_ACHIEVEMENT_NAME";
        }


        private static string GetEffectiveAchievementDescriptionToken(
            string bodyName
        )
        {
            return
                $"USU_{MakeToken(bodyName)}_EFFECTIVE_ACHIEVEMENT_DESCRIPTION";
        }


        private static string EscapeForLog(
            string value
        )
        {
            return (value ?? "")
                .Replace("\r", "")
                .Replace("\n", "\\n");
        }


        private static string FormatPresentationReason(
            string reason
        )
        {
            return
                string.IsNullOrWhiteSpace(reason)
                    ? ""
                    : " | Motivo: " + reason.Trim();
        }
    }
}
