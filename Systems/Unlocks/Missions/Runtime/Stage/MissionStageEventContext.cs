using RoR2;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * CONTEXTO DE EVENTO DE SECTOR
     * =============================================================
     *
     * Objeto pequeño e inmutable que describe:
     *
     * - qué sector está activo,
     * - qué escena Unity representa ese sector,
     * - qué número de sector/evento llevamos en la run,
     * - y, en caso de finalización, qué teletransportador
     *   produjo el evento.
     *
     * No contiene lógica de Wooper ni de ningún survivor.
     * =============================================================
     */
    public sealed class MissionStageEventContext
    {
        public string StageName
        {
            get;
        }


        public int SceneHandle
        {
            get;
        }


        public int StageSequence
        {
            get;
        }


        public TeleporterInteraction Teleporter
        {
            get;
        }


        public MissionStageEventContext(
            string stageName,
            int sceneHandle,
            int stageSequence,
            TeleporterInteraction teleporter = null
        )
        {
            StageName =
                stageName ?? "";

            SceneHandle =
                sceneHandle;

            StageSequence =
                stageSequence;

            Teleporter =
                teleporter;
        }
    }
}
