using R2API.Networking.Interfaces;

using UnityEngine;
using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    // =============================================================
    // BANDIT — RESULTADO DE LIGHTS OUT
    // =============================================================

    public sealed class HunkBanditShotResultMessage :
        INetMessage
    {
        public GameObject BodyObject;

        public bool Success;


        public HunkBanditShotResultMessage()
        {
        }


        public HunkBanditShotResultMessage(
            GameObject bodyObject,
            bool success
        )
        {
            BodyObject =
                bodyObject;

            Success =
                success;
        }


        public void Serialize(
            NetworkWriter writer
        )
        {
            writer.Write(
                BodyObject
            );


            writer.Write(
                Success
            );
        }


        public void Deserialize(
            NetworkReader reader
        )
        {
            BodyObject =
                reader.ReadGameObject();


            Success =
                reader.ReadBoolean();
        }


        public void OnReceived()
        {
            /*
             * Este mensaje sólo nos interesa
             * cuando llega al servidor.
             */
            if (!NetworkServer.active)
            {
                return;
            }


            PrecisionExecutionStreakTracker
                .RegisterRemoteBanditShotResult(
                    BodyObject,
                    Success
                );
        }
    }


    // =============================================================
    // RAILGUNNER — RESULTADO DEL WEAK POINT
    // =============================================================

    public sealed class HunkRailgunnerShotResultMessage :
        INetMessage
    {
        public GameObject BodyObject;

        public bool Success;


        public HunkRailgunnerShotResultMessage()
        {
        }


        public HunkRailgunnerShotResultMessage(
            GameObject bodyObject,
            bool success
        )
        {
            BodyObject =
                bodyObject;

            Success =
                success;
        }


        public void Serialize(
            NetworkWriter writer
        )
        {
            writer.Write(
                BodyObject
            );


            writer.Write(
                Success
            );
        }


        public void Deserialize(
            NetworkReader reader
        )
        {
            BodyObject =
                reader.ReadGameObject();


            Success =
                reader.ReadBoolean();
        }


        public void OnReceived()
        {
            if (!NetworkServer.active)
            {
                return;
            }


            PrecisionExecutionStreakTracker
                .RegisterRemoteRailgunnerShot(
                    BodyObject,
                    Success
                );
        }
    }
}