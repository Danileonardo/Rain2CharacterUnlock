using BepInEx.Configuration;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Opciones temporales de validación de 0.3.1.
    ///
    /// GenerateAlongsideAuthored permite construir un slot Generated dormido
    /// también para survivors modded que ya poseen Original o una misión USU.
    /// No sustituye la autoridad activa: sólo crea el slot alternativo para
    /// probar coexistencia, reroll y borrado antes de la publicación.
    ///
    /// RELEASE BLOCKER: volver a false/eliminar esta política antes del paquete
    /// público si se decide mantener Generated exclusivamente como fallback.
    /// </summary>
    public static class TemporaryGeneratedTestOptions
    {
        private static ConfigEntry<bool> generateAlongsideAuthored;

        public static bool GenerateAlongsideAuthored =>
            generateAlongsideAuthored != null &&
            generateAlongsideAuthored.Value;

        public static ConfigEntry<bool> GenerateAlongsideAuthoredEntry =>
            generateAlongsideAuthored;

        public static void Initialize(ConfigFile config)
        {
            if (config == null)
            {
                return;
            }

            generateAlongsideAuthored =
                config.Bind(
                    "Testing",
                    "GenerateAlongsideAuthored",
                    false,
                    "DEV: pre-genera también slots Generated para personajes modded con Original/USU. Normalmente debe quedar desactivado: la pestaña Generada permite crearlos manualmente cuando se necesiten."
                );
        }

        public static void ToggleGenerateAlongsideAuthored()
        {
            if (generateAlongsideAuthored != null)
            {
                generateAlongsideAuthored.Value = !generateAlongsideAuthored.Value;
            }
        }


        public static bool CanGenerateAlongside(
            GeneratedMissionAuthorityKind authority
        )
        {
            if (!GenerateAlongsideAuthored)
            {
                return false;
            }

            return
                authority == GeneratedMissionAuthorityKind.OriginalUnlock ||
                authority == GeneratedMissionAuthorityKind.UsuOfficialPreset ||
                authority == GeneratedMissionAuthorityKind.ExistingUsuMission;
        }
    }
}
