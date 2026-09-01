using BepInEx.Logging;

using RoR2;

using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    public static class SessionUnlockAnnouncementManager
    {
        /*
         * Token utilizado por el juego para el mensaje de chat
         * de un achievement obtenido.
         *
         * La intención es conservar el formato/localización vanilla
         * en lugar de construir manualmente una frase en español.
         */
        private const string VanillaAchievementUnlockedToken =
            "ACHIEVEMENT_UNLOCKED_MESSAGE";


        public static void AnnounceAchievement(
            NetworkUser networkUser,
            string bodyName,
            ManualLogSource logger
        )
        {
            if (!NetworkServer.active)
            {
                return;
            }


            if (networkUser == null)
            {
                logger?.LogWarning(
                    "[SESSION UNLOCK CHAT] NetworkUser nulo | " +
                    $"Body: {bodyName}"
                );

                return;
            }


            if (
                !SurvivorUnlockManager
                    .TryGetCustomAchievement(
                        bodyName,
                        out AchievementDef achievementDef
                    ) ||
                achievementDef == null
            )
            {
                logger?.LogWarning(
                    "[SESSION UNLOCK CHAT] Achievement USU no encontrado | " +
                    $"Body: {bodyName}"
                );

                return;
            }


            CharacterBody currentBody =
                networkUser.GetCurrentBody();


            if (currentBody == null)
            {
                logger?.LogWarning(
                    "[SESSION UNLOCK CHAT] El jugador no tiene body actual; " +
                    "no se pudo construir el SubjectFormatChatMessage | " +
                    $"Jugador: {networkUser.userName} | " +
                    $"Body recompensa: {bodyName}"
                );

                return;
            }


            Chat.SubjectFormatChatMessage message =
                new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody =
                        currentBody,

                    baseToken =
                        VanillaAchievementUnlockedToken,

                    paramTokens =
                        new string[]
                        {
                            achievementDef.nameToken
                        }
                };


            Chat.SendBroadcastChat(
                message
            );


            logger?.LogInfo(
                "[SESSION UNLOCK CHAT] Achievement anunciado | " +
                $"Jugador: {networkUser.userName} | " +
                $"Body: {bodyName} | " +
                $"Achievement: {achievementDef.identifier}"
            );
        }
    }
}
