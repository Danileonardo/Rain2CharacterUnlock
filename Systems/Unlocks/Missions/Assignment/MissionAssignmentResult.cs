namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Resultado estable de una operación/validación de asignación.
    ///
    /// La futura UI no debe necesitar interpretar excepciones ni conocer
    /// detalles internos de la biblioteca. Puede reaccionar usando Code y
    /// mostrar Message al jugador cuando corresponda.
    /// </summary>
    public sealed class MissionAssignmentResult
    {
        public bool Success
        {
            get;
            private set;
        }


        public string Code
        {
            get;
            private set;
        } = "Unknown";


        public string Message
        {
            get;
            private set;
        } = "";


        public string BodyName
        {
            get;
            private set;
        } = "";


        public string PresetId
        {
            get;
            private set;
        } = "";


        public string TargetDisplayName
        {
            get;
            private set;
        } = "";


        public string PresetName
        {
            get;
            private set;
        } = "";


        private MissionAssignmentResult()
        {
        }


        public static MissionAssignmentResult CreateSuccess(
            string bodyName,
            string presetId,
            string targetDisplayName,
            string presetName,
            string message
        )
        {
            return new MissionAssignmentResult
            {
                Success = true,
                Code = MissionAssignmentResultCodes.Success,
                Message = message ?? "",
                BodyName = bodyName ?? "",
                PresetId = presetId ?? "",
                TargetDisplayName = targetDisplayName ?? "",
                PresetName = presetName ?? ""
            };
        }


        public static MissionAssignmentResult CreateFailure(
            string code,
            string message,
            string bodyName = "",
            string presetId = ""
        )
        {
            return new MissionAssignmentResult
            {
                Success = false,
                Code = string.IsNullOrWhiteSpace(code)
                    ? MissionAssignmentResultCodes.Unknown
                    : code,
                Message = message ?? "",
                BodyName = bodyName ?? "",
                PresetId = presetId ?? ""
            };
        }
    }


    /// <summary>
    /// Códigos estables para UI, logs y futuras pruebas.
    /// Se mantienen como strings para poder evolucionar el servicio sin
    /// acoplar el JSON o la interfaz a un enum serializado.
    /// </summary>
    public static class MissionAssignmentResultCodes
    {
        public const string Success =
            "Success";

        public const string Unknown =
            "Unknown";

        public const string InvalidBodyName =
            "InvalidBodyName";

        public const string SurvivorNotFound =
            "SurvivorNotFound";

        public const string InvalidPresetId =
            "InvalidPresetId";

        public const string PresetNotAssignable =
            "PresetNotAssignable";

        public const string OriginalProviderProtected =
            "OriginalProviderProtected";

        public const string InvalidMission =
            "InvalidMission";

        public const string CustomMissionNotAvailable =
            "CustomMissionNotAvailable";

        public const string PersistenceFailed =
            "PersistenceFailed";
    }
}
