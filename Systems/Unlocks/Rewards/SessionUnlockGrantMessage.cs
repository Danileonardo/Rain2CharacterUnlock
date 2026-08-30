using R2API.Networking.Interfaces;

using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    public sealed class SessionUnlockGrantMessage
        : INetMessage
    {
        public string BodyName;


        // =========================================================
        // CONSTRUCTOR VACÍO
        // =========================================================

        public SessionUnlockGrantMessage()
        {
        }


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public SessionUnlockGrantMessage(
            string bodyName
        )
        {
            BodyName =
                bodyName;
        }


        // =========================================================
        // SERIALIZAR
        // =========================================================

        public void Serialize(
            NetworkWriter writer
        )
        {
            writer.Write(
                BodyName ??
                string.Empty
            );
        }


        // =========================================================
        // DESERIALIZAR
        // =========================================================

        public void Deserialize(
            NetworkReader reader
        )
        {
            BodyName =
                reader.ReadString();
        }


        // =========================================================
        // RECIBIR
        // =========================================================

        public void OnReceived()
        {
            /*
             * El host ya validó la misión.
             *
             * Cada cliente revisa SU propio
             * UserProfile y solamente concede
             * aquello que le falte.
             */

            SessionUnlockManager.GrantLocally(
                BodyName
            );
        }
    }
}