using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * CLIENTE -> HOST
     * =============================================================
     *
     * Petición EVENT-DRIVEN del snapshot del lobby.
     *
     * Se envía cuando la pantalla de selección de personajes
     * del cliente aparece.
     *
     * No es un sistema de polling.
     * No se utiliza para reconstruir progreso a mitad de run.
     * =============================================================
     */
    public sealed class SessionMissionLobbyRequestMessage :
        INetMessage
    {
        public SessionMissionLobbyRequestMessage()
        {
        }


        public void Serialize(
            NetworkWriter writer
        )
        {
            /*
             * Sin payload.
             */
        }


        public void Deserialize(
            NetworkReader reader
        )
        {
            /*
             * Sin payload.
             */
        }


        public void OnReceived()
        {
            if (!NetworkServer.active)
            {
                return;
            }


            SessionMissionLobbySyncManager
                .ReceiveClientLobbySnapshotRequest();
        }
    }
}
