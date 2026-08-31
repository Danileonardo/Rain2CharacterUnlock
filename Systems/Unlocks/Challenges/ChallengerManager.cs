using System;


namespace UniversalSurvivorUnlocks
{
    public static class ChallengeManager
    {
        // =========================================================
        // ACHIEVEMENT USU
        // =========================================================
        //
        // El Achievement existe para:
        //
        // - aparecer correctamente en el sistema de desbloqueos;
        // - almacenar el estado en UserProfile;
        // - conservar nombre, descripción e icono;
        // - estar asociado al UnlockableDef.
        //
        // YA NO comprueba la misión por sí mismo.
        //
        // =========================================================

        public static Type GetAchievementType(
            SurvivorChallengeJson challenge
        )
        {
            return typeof(
                UniversalSurvivorAchievement
            );
        }


        // =========================================================
        // SERVER ACHIEVEMENT
        // =========================================================
        //
        // Antes cada misión tenía un:
        //
        // XxxServerAchievement
        //     ↓
        // Grant()
        //
        // Ahora toda finalización pasa solamente por:
        //
        // Tracker
        //     ↓
        // ChallengeCompletionRouter
        //     ↓
        // SessionUnlockManager
        //
        // =========================================================

        public static Type GetServerTrackerType(
            SurvivorChallengeJson challenge
        )
        {
            return null;
        }
    }
}