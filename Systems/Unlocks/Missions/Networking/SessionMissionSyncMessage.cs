using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * HOST -> CLIENTES
     * =============================================================
     *
     * Transporta un snapshot completo de misiones.
     *
     * IsRunSnapshot = false
     *     Snapshot del lobby.
     *
     * IsRunSnapshot = true
     *     Snapshot congelado de la run.
     *
     * El cliente mantiene estos datos únicamente en memoria.
     * NUNCA se escriben en su Survivors.json.
     * =============================================================
     */
    public sealed class SessionMissionSyncMessage :
        INetMessage
    {
        public bool IsRunSnapshot;

        public string SnapshotJson;


        public SessionMissionSyncMessage()
        {
        }


        public SessionMissionSyncMessage(
            string snapshotJson,
            bool isRunSnapshot
        )
        {
            SnapshotJson =
                snapshotJson ?? "";

            IsRunSnapshot =
                isRunSnapshot;
        }


        public void Serialize(
            NetworkWriter writer
        )
        {
            writer.Write(
                IsRunSnapshot
            );

            writer.Write(
                SnapshotJson ?? ""
            );
        }


        public void Deserialize(
            NetworkReader reader
        )
        {
            IsRunSnapshot =
                reader.ReadBoolean();

            SnapshotJson =
                reader.ReadString();
        }


        public void OnReceived()
        {
            if (NetworkServer.active)
            {
                return;
            }


            SessionMissionRegistry
                .ReceiveHostSnapshot(
                    SnapshotJson,
                    IsRunSnapshot
                );
        }
    }
}
