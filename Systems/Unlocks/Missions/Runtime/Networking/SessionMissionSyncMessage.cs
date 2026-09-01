using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * HOST -> CLIENTES
     * =============================================================
     *
     * Transporta el snapshot completo de misiones de la run.
     *
     * Sólo contiene JSON temporal.
     * El cliente NO escribe ese contenido en Survivors.json.
     */
    public sealed class SessionMissionSyncMessage :
        INetMessage
    {
        public string SnapshotJson;


        public SessionMissionSyncMessage()
        {
        }


        public SessionMissionSyncMessage(
            string snapshotJson
        )
        {
            SnapshotJson =
                snapshotJson ?? "";
        }


        public void Serialize(
            NetworkWriter writer
        )
        {
            writer.Write(
                SnapshotJson ?? ""
            );
        }


        public void Deserialize(
            NetworkReader reader
        )
        {
            SnapshotJson =
                reader.ReadString();
        }


        public void OnReceived()
        {
            /*
             * El host ya tiene su snapshot local.
             * Este mensaje existe únicamente para clientes.
             */
            if (NetworkServer.active)
            {
                return;
            }


            SessionMissionRegistry
                .ReceiveHostSnapshot(
                    SnapshotJson
                );
        }
    }
}
