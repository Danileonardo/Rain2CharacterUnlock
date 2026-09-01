using System.Collections.Generic;

using R2API.Networking;
using R2API.Networking.Interfaces;

using RoR2;

using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    /*
     * HOST -> CLIENTES
     *
     * El host ya validó que la misión terminó.
     * Cada cliente concede/verifica la recompensa en SU UserProfile.
     */
    public sealed class SessionUnlockGrantMessage
        : INetMessage
    {
        public string BodyName;


        public SessionUnlockGrantMessage()
        {
        }


        public SessionUnlockGrantMessage(
            string bodyName
        )
        {
            BodyName =
                bodyName;
        }


        public void Serialize(
            NetworkWriter writer
        )
        {
            writer.Write(
                BodyName ??
                string.Empty
            );
        }


        public void Deserialize(
            NetworkReader reader
        )
        {
            BodyName =
                reader.ReadString();
        }


        public void OnReceived()
        {
            /*
             * El host ya ejecutó GrantLocally() directamente.
             *
             * Si este mensaje llegara también al cliente local del host,
             * no debemos volver a ejecutar la concesión ni mandar un ACK
             * duplicado hacia el propio servidor.
             */
            if (NetworkServer.active)
            {
                return;
            }


            List<SessionUnlockGrantResult> results =
                SessionUnlockManager
                    .GrantLocally(
                        BodyName
                    );


            foreach (
                SessionUnlockGrantResult result
                in results
            )
            {
                NetworkUser networkUser =
                    result
                        .LocalUser?
                        .currentNetworkUser;


                if (networkUser == null)
                {
                    continue;
                }


                new SessionUnlockResultMessage(
                    networkUser.gameObject,
                    result.BodyName,
                    result.AchievementBefore,
                    result.UnlockableBefore,
                    result.AchievementAfter,
                    result.UnlockableAfter
                )
                .Send(
                    NetworkDestination.Server
                );
            }
        }
    }
}
