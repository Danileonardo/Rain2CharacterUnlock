using R2API.Networking.Interfaces;

using UnityEngine;
using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    /*
     * CLIENTE -> HOST
     *
     * Mismo principio que usamos con HUNK:
     * el cliente comprueba localmente lo que realmente ocurrió
     * y después informa el resultado al host.
     */
    public sealed class SessionUnlockResultMessage
        : INetMessage
    {
        public GameObject NetworkUserObject;

        public string BodyName;

        public bool AchievementBefore;

        public bool UnlockableBefore;

        public bool AchievementAfter;

        public bool UnlockableAfter;


        public SessionUnlockResultMessage()
        {
        }


        public SessionUnlockResultMessage(
            GameObject networkUserObject,
            string bodyName,
            bool achievementBefore,
            bool unlockableBefore,
            bool achievementAfter,
            bool unlockableAfter
        )
        {
            NetworkUserObject =
                networkUserObject;

            BodyName =
                bodyName;

            AchievementBefore =
                achievementBefore;

            UnlockableBefore =
                unlockableBefore;

            AchievementAfter =
                achievementAfter;

            UnlockableAfter =
                unlockableAfter;
        }


        public void Serialize(
            NetworkWriter writer
        )
        {
            writer.Write(
                NetworkUserObject
            );

            writer.Write(
                BodyName ??
                string.Empty
            );

            writer.Write(
                AchievementBefore
            );

            writer.Write(
                UnlockableBefore
            );

            writer.Write(
                AchievementAfter
            );

            writer.Write(
                UnlockableAfter
            );
        }


        public void Deserialize(
            NetworkReader reader
        )
        {
            NetworkUserObject =
                reader.ReadGameObject();

            BodyName =
                reader.ReadString();

            AchievementBefore =
                reader.ReadBoolean();

            UnlockableBefore =
                reader.ReadBoolean();

            AchievementAfter =
                reader.ReadBoolean();

            UnlockableAfter =
                reader.ReadBoolean();
        }


        public void OnReceived()
        {
            if (!NetworkServer.active)
            {
                return;
            }


            SessionUnlockManager
                .ReceiveClientGrantResult(
                    NetworkUserObject,
                    BodyName,
                    AchievementBefore,
                    UnlockableBefore,
                    AchievementAfter,
                    UnlockableAfter
                );
        }
    }
}
