using System;
using Newtonsoft.Json;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Snapshot mínimo de persistencia de misión.
    ///
    /// No contiene skills, skins, estadísticas, prefabs, lore ni el perfil
    /// runtime del personaje. Su propósito es separar explícitamente lo que USU
    /// necesita conservar cuando un mod desaparece: identidad mínima + misión.
    ///
    /// G2 utiliza esta abstracción como almacenamiento real de contenido ausente
    /// dentro de dormantMissions. Al reinstalarse el mod, la misión se rehidrata
    /// sobre una entrada reconstruida desde el contenido runtime actual.
    /// </summary>
    public sealed class SurvivorMissionRecord
    {
        public const string PendingContentState = "PendingContent";

        [JsonProperty("survivorId", Order = 1)]
        public string SurvivorId
        {
            get;
            set;
        } = "";

        [JsonProperty("bodyName", Order = 2)]
        public string BodyName
        {
            get;
            set;
        } = "";

        [JsonProperty("source", Order = 3)]
        public string Source
        {
            get;
            set;
        } = "";

        [JsonProperty("state", Order = 4)]
        public string State
        {
            get;
            set;
        } = PendingContentState;

        [JsonProperty("challenge", Order = 5)]
        public SurvivorChallengeJson Challenge
        {
            get;
            set;
        }


        public static SurvivorMissionRecord FromEntry(
            string dictionaryKey,
            SurvivorJsonEntry entry
        )
        {
            if (entry == null)
            {
                return null;
            }

            string bodyName =
                !string.IsNullOrWhiteSpace(
                    entry.BodyName
                )
                    ? entry.BodyName
                    : dictionaryKey ?? "";

            string survivorId =
                entry.SurvivorId;

            if (
                string.IsNullOrWhiteSpace(
                    survivorId
                ) ||
                SurvivorIdentity.IsBodyFallback(
                    survivorId
                )
            )
            {
                string resolved =
                    SurvivorIdentity.Build(
                        entry.Source,
                        bodyName
                    );

                if (!string.IsNullOrWhiteSpace(resolved))
                {
                    survivorId = resolved;
                }
            }

            return
                new SurvivorMissionRecord
                {
                    SurvivorId = survivorId ?? "",
                    BodyName = bodyName,
                    Source = entry.Source ?? "",
                    State = PendingContentState,
                    Challenge = entry.Challenge
                };
        }


        public void ApplyMissionTo(
            SurvivorJsonEntry entry
        )
        {
            if (entry == null)
            {
                return;
            }

            if (
                !string.IsNullOrWhiteSpace(
                    SurvivorId
                )
            )
            {
                entry.SurvivorId =
                    SurvivorId;
            }

            if (Challenge != null)
            {
                entry.Challenge =
                    Challenge;
            }
        }
    }
}
