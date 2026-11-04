using System.Collections.Generic;

using R2API.Networking;
using R2API.Networking.Interfaces;

using RoR2;
using UnityEngine;

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
        // null = difusión cooperativa antigua (sin cambio).
        public GameObject RecipientNetworkUserObject;
        public bool IsIndividual;
        public string RecipientIdentityKey;
        // Solo se usa para notificación local de HUNK. El host valida la ruta.
        public string HunkMedalOutcome;


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

        public SessionUnlockGrantMessage(string bodyName, string hunkMedalOutcome)
        {
            BodyName = bodyName;
            HunkMedalOutcome = hunkMedalOutcome;
        }

        public SessionUnlockGrantMessage(string bodyName, GameObject recipient)
        {
            BodyName = bodyName;
            RecipientNetworkUserObject = recipient;
            IsIndividual = true;
            NetworkUser user = recipient == null ? null : recipient.GetComponent<NetworkUser>();
            MissionPlayerIdentity identity;
            if (MissionPlayerIdentity.TryCreate(user, out identity))
                RecipientIdentityKey = identity.Key;
        }


        // Permite conservar las medallas de HUNK incluso cuando la entrega
        // es selectiva por jugador (Mixed) y no un broadcast de sesión.
        public SessionUnlockGrantMessage(
            string bodyName, GameObject recipient, string hunkMedalOutcome)
            : this(bodyName, recipient)
        {
            HunkMedalOutcome = hunkMedalOutcome;
        }

        public void Serialize(
            NetworkWriter writer
        )
        {
            writer.Write(
                BodyName ??
                string.Empty
            );
            writer.Write(IsIndividual);
            writer.Write(RecipientNetworkUserObject);
            writer.Write(RecipientIdentityKey ?? string.Empty);
            writer.Write(HunkMedalOutcome ?? string.Empty);
        }


        public void Deserialize(
            NetworkReader reader
        )
        {
            BodyName =
                reader.ReadString();
            IsIndividual = reader.ReadBoolean();
            RecipientNetworkUserObject = reader.ReadGameObject();
            RecipientIdentityKey = reader.ReadString();
            HunkMedalOutcome = reader.ReadString();
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


            if (IsIndividual && RecipientNetworkUserObject == null &&
                string.IsNullOrWhiteSpace(RecipientIdentityKey))
            {
                // Nunca degradar una recompensa personal a broadcast.
                return;
            }

            List<SessionUnlockGrantResult> results =
                SessionUnlockManager
                    .GrantLocally(
                        BodyName,
                        RecipientNetworkUserObject,
                        HunkMedalOutcome,
                        IsIndividual ? RecipientIdentityKey : null
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
