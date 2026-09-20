using System;
using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// G4 Phase 1.4 - Community Evidence Calibration.
    ///
    /// Catálogo local/offline de evidencia comunitaria. Las fuentes se
    /// investigan fuera del juego y se empaquetan como datos auditables.
    /// Nunca hay scraping o acceso de red durante una partida.
    ///
    /// Dos principios evitan que Reddit/TierMaker sustituyan a G4 local:
    /// - hacen falta al menos dos grupos independientes para dejar Unknown;
    /// - un hilo/lista cuenta una sola vez aunque contenga varias opiniones.
    /// </summary>
    public sealed class CommunityPowerCatalog : ICommunityPowerProvider
    {
        public static CommunityPowerCatalog Instance
        {
            get;
        } = new CommunityPowerCatalog();

        private const int MinimumComparableSources = 2;

        private readonly Dictionary<string, List<CommunityPowerSource>>
            sourcesBySurvivor =
                new Dictionary<string, List<CommunityPowerSource>>(
                    StringComparer.OrdinalIgnoreCase
                );

        private CommunityPowerCatalog()
        {
            SeedCuratedEvidence();
        }


        public void Register(
            CommunityPowerSource source
        )
        {
            if (
                source == null ||
                string.IsNullOrWhiteSpace(source.SurvivorId) ||
                source.Tier == CommunityPowerTier.Unknown ||
                !source.IsPowerFocused
            )
            {
                return;
            }

            List<CommunityPowerSource> list;

            if (!sourcesBySurvivor.TryGetValue(source.SurvivorId, out list))
            {
                list = new List<CommunityPowerSource>();
                sourcesBySurvivor[source.SurvivorId] = list;
            }

            if (!string.IsNullOrWhiteSpace(source.SourceId))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    CommunityPowerSource existing = list[i];

                    if (
                        existing != null &&
                        string.Equals(
                            existing.SourceId,
                            source.SourceId,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        return;
                    }
                }
            }

            source.Weight = Clamp(source.Weight, 0.10f, 1f);
            list.Add(source);
        }


        public bool TryGet(
            string survivorId,
            out CommunityPowerAssessment assessment
        )
        {
            assessment =
                new CommunityPowerAssessment
                {
                    SurvivorId = survivorId ?? "",
                    Tier = CommunityPowerTier.Unknown,
                    SourceCount = 0,
                    IndependentSourceCount = 0,
                    Confidence = 0f,
                    Consensus = 0f,
                    IsKnown = false
                };

            if (string.IsNullOrWhiteSpace(survivorId))
            {
                return false;
            }

            List<CommunityPowerSource> sources;

            if (!sourcesBySurvivor.TryGetValue(survivorId, out sources))
            {
                return false;
            }

            Dictionary<string, GroupAccumulator> groups =
                new Dictionary<string, GroupAccumulator>(
                    StringComparer.OrdinalIgnoreCase
                );

            int validCount = 0;

            for (int i = 0; i < sources.Count; i++)
            {
                CommunityPowerSource source = sources[i];

                if (
                    source == null ||
                    source.Tier == CommunityPowerTier.Unknown ||
                    !source.IsPowerFocused
                )
                {
                    continue;
                }

                string groupId =
                    !string.IsNullOrWhiteSpace(source.IndependentGroupId)
                        ? source.IndependentGroupId
                        : source.SourceId;

                if (string.IsNullOrWhiteSpace(groupId))
                {
                    groupId = "anonymous:" + i;
                }

                GroupAccumulator group;

                if (!groups.TryGetValue(groupId, out group))
                {
                    group = new GroupAccumulator();
                    groups[groupId] = group;
                }

                float weight = Clamp(source.Weight, 0.10f, 1f);
                group.WeightedTier += (int)source.Tier * weight;
                group.TotalWeight += weight;
                group.SourceCount++;
                validCount++;
            }

            assessment.SourceCount = validCount;
            assessment.IndependentSourceCount = groups.Count;

            if (
                validCount < MinimumComparableSources ||
                groups.Count < MinimumComparableSources
            )
            {
                return false;
            }

            float weightedTier = 0f;
            float totalGroupWeight = 0f;
            List<GroupScore> groupScores = new List<GroupScore>();

            foreach (KeyValuePair<string, GroupAccumulator> pair in groups)
            {
                GroupAccumulator group = pair.Value;

                if (
                    group == null ||
                    group.SourceCount <= 0 ||
                    group.TotalWeight <= 0f
                )
                {
                    continue;
                }

                float groupTier = group.WeightedTier / group.TotalWeight;

                /*
                 * Cada hilo/lista tiene una sola voz independiente. Su peso
                 * es la calidad MEDIA de sus observaciones, no la cantidad de
                 * comentarios que contenga.
                 */
                float groupWeight =
                    Clamp(
                        group.TotalWeight / group.SourceCount,
                        0.10f,
                        1f
                    );

                groupScores.Add(
                    new GroupScore
                    {
                        Tier = groupTier,
                        Weight = groupWeight
                    }
                );

                weightedTier += groupTier * groupWeight;
                totalGroupWeight += groupWeight;
            }

            if (
                groupScores.Count < MinimumComparableSources ||
                totalGroupWeight <= 0f
            )
            {
                return false;
            }

            float mean = weightedTier / totalGroupWeight;
            float disagreement = 0f;

            for (int i = 0; i < groupScores.Count; i++)
            {
                GroupScore group = groupScores[i];
                disagreement +=
                    Math.Abs(group.Tier - mean) * group.Weight;
            }

            disagreement /= totalGroupWeight;

            float consensus =
                Clamp(
                    1f - disagreement / 2f,
                    0f,
                    1f
                );

            float sourceDepth =
                Clamp(
                    groupScores.Count / 4f,
                    0f,
                    1f
                );

            float sourceQuality =
                Clamp(
                    totalGroupWeight / groupScores.Count,
                    0f,
                    1f
                );

            float confidence =
                Clamp(
                    sourceDepth *
                    (0.55f + 0.45f * consensus) *
                    (0.70f + 0.30f * sourceQuality),
                    0f,
                    1f
                );

            int resolvedTier =
                (int)Math.Floor(mean + 0.5f);

            resolvedTier =
                Math.Max(1, Math.Min(5, resolvedTier));

            assessment.Tier = (CommunityPowerTier)resolvedTier;
            assessment.Consensus = consensus;
            assessment.Confidence = confidence;
            assessment.IsKnown = true;

            return true;
        }


        public IReadOnlyList<CommunityPowerSource> GetSources(
            string survivorId
        )
        {
            List<CommunityPowerSource> sources;

            if (
                string.IsNullOrWhiteSpace(survivorId) ||
                !sourcesBySurvivor.TryGetValue(survivorId, out sources) ||
                sources == null
            )
            {
                return new CommunityPowerSource[0];
            }

            return sources;
        }


        public int RegisteredSurvivorCount
        {
            get
            {
                return sourcesBySurvivor.Count;
            }
        }


        public int RegisteredSourceCount
        {
            get
            {
                int count = 0;

                foreach (
                    KeyValuePair<string, List<CommunityPowerSource>> pair
                    in sourcesBySurvivor
                )
                {
                    if (pair.Value != null)
                    {
                        count += pair.Value.Count;
                    }
                }

                return count;
            }
        }


        public int MinimumIndependentSources
        {
            get
            {
                return MinimumComparableSources;
            }
        }


        private void SeedCuratedEvidence()
        {
            /*
             * Normalización editorial estable:
             * C     = claramente muy débil / muy por debajo de referencia
             * B     = débil / por debajo de referencia
             * A     = equilibrado / vanilla-like / no OP
             * S     = fuerte / overtuned / demasiado fuerte
             * SPlus = explícitamente roto / overpowered
             *
             * Un survivor puede tener evidencia registrada y seguir Unknown
             * si procede de menos de dos grupos independientes.
             */

            // Enforcer ----------------------------------------------------
            Seed(
                "survivor:enforcer.enforcercontent:enforcerbody",
                "reddit.2026.enforcer.balanced",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.A,
                "reddit:riskofrain:1tt14c9",
                "Most balanced modded survivor?",
                "https://www.reddit.com/r/riskofrain/comments/1tt14c9/",
                "Fairly balanced; exceptional tanking offset by slow movement and no mobility skills.",
                "balanced -> A",
                "General",
                "",
                "2026-05-31",
                0.95f
            );
            Seed(
                "survivor:enforcer.enforcercontent:enforcerbody",
                "reddit.2026.enforcer.tanking-disagreement",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.S,
                "reddit:riskofrain:1tt14c9",
                "Most balanced modded survivor?",
                "https://www.reddit.com/r/riskofrain/comments/1tt14c9/",
                "Counter-opinion in the same thread: can tank almost everything and does not feel balanced.",
                "explicit balance concern / too strong -> S",
                "General",
                "",
                "2026-05-31",
                0.55f
            );
            Seed(
                "survivor:enforcer.enforcercontent:enforcerbody",
                "steam.2024.enforcer.balanced",
                CommunityPowerSourceKind.GuideOrReview,
                CommunityPowerTier.A,
                "steam:632360:4428814237729615874",
                "Most balanced mod only characters",
                "https://steamcommunity.com/app/632360/discussions/0/4428814237729615874/",
                "Experienced modded-player list includes Enforcer among nicely balanced survivors.",
                "nicely balanced -> A",
                "General",
                "",
                "2024-08-26",
                0.80f
            );
            Seed(
                "survivor:enforcer.enforcercontent:enforcerbody",
                "steam.2024.enforcer.shield-busted",
                CommunityPowerSourceKind.GuideOrReview,
                CommunityPowerTier.S,
                "steam:632360:4428814237729615874",
                "Most balanced mod only characters",
                "https://steamcommunity.com/app/632360/discussions/0/4428814237729615874/",
                "Counter-opinion in the same discussion calls the directional shield busted/easy.",
                "busted defensive mechanic -> S",
                "General",
                "",
                "2024-08-26",
                0.55f
            );
            Seed(
                "survivor:enforcer.enforcercontent:enforcerbody",
                "reddit.2023.enforcer.bit-op",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.S,
                "reddit:riskofrain:12r5hh5",
                "Favorite Modded class?",
                "https://www.reddit.com/r/riskofrain/comments/12r5hh5/",
                "Older opinion: Enforcer might be a bit OP because Protect and Serve blocks almost any damage when positioned correctly.",
                "bit OP -> S; old/version-unknown evidence heavily down-weighted",
                "General",
                "",
                "2023-04-18",
                0.30f
            );

            // Rocket ------------------------------------------------------
            Seed(
                "survivor:com.enforcergang.rocketsurvivor:rocketsurvivorbody",
                "reddit.2026.rocket.balanced",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.A,
                "reddit:riskofrain:1tt14c9",
                "Most balanced modded survivor?",
                "https://www.reddit.com/r/riskofrain/comments/1tt14c9/",
                "Explicitly nominated as especially well balanced.",
                "balanced -> A",
                "General",
                "",
                "2026-05-31",
                1.00f
            );
            Seed(
                "survivor:com.enforcergang.rocketsurvivor:rocketsurvivorbody",
                "reddit.2025.rocket.balanced-high-ceiling",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.A,
                "reddit:riskofrain:1o2pyrw",
                "balanced survival mods",
                "https://www.reddit.com/r/riskofrain/comments/1o2pyrw/",
                "Called very balanced while retaining a high skill ceiling.",
                "very balanced -> A",
                "General",
                "",
                "2025-10-10",
                0.85f
            );
            Seed(
                "survivor:com.enforcergang.rocketsurvivor:rocketsurvivorbody",
                "reddit.2025.rocket.too-strong-e8",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.S,
                "reddit:riskofrain:1m34djv",
                "Modded Survivors to recommend?",
                "https://www.reddit.com/r/riskofrain/comments/1m34djv/",
                "Experienced Eclipse player calls Rocket really strong/too strong and reports breezing through E8 first try.",
                "too strong -> S",
                "Eclipse 8",
                "",
                "2025-07-18",
                0.95f
            );
            Seed(
                "survivor:com.enforcergang.rocketsurvivor:rocketsurvivorbody",
                "reddit.2023.rocket.overtuned",
                CommunityPowerSourceKind.GuideOrReview,
                CommunityPowerTier.S,
                "reddit:riskofrain:166mtc0",
                "modded survivor review: rocket",
                "https://www.reddit.com/r/riskofrain/comments/166mtc0/",
                "Detailed review calls Rocket a touch overtuned and too much of a cakewalk while praising the design.",
                "overtuned -> S; older evidence reduced",
                "General",
                "",
                "2023-08-31",
                0.80f
            );
            Seed(
                "survivor:com.enforcergang.rocketsurvivor:rocketsurvivorbody",
                "steam.2024.rocket.balanced",
                CommunityPowerSourceKind.GuideOrReview,
                CommunityPowerTier.A,
                "steam:632360:4428814237729615874",
                "Most balanced mod only characters",
                "https://steamcommunity.com/app/632360/discussions/0/4428814237729615874/",
                "Multiple recommendations describe Rocket as fun/balanced and include him in a nicely balanced survivor list.",
                "balanced -> A",
                "General",
                "",
                "2024-08-26",
                0.85f
            );

            // Miner -------------------------------------------------------
            Seed(
                "survivor:diggerunearthed.diggercontent:minerbody",
                "reddit.2026.miner.balanced-list",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.A,
                "reddit:riskofrain:1tt14c9",
                "Most balanced modded survivor?",
                "https://www.reddit.com/r/riskofrain/comments/1tt14c9/",
                "Included in a short list answering a request for survivors between too weak and overpowered.",
                "balanced recommendation -> A",
                "General",
                "",
                "2026-05-31",
                0.95f
            );
            Seed(
                "survivor:diggerunearthed.diggercontent:minerbody",
                "steam.2024.miner.balanced",
                CommunityPowerSourceKind.GuideOrReview,
                CommunityPowerTier.A,
                "steam:632360:4428814237729615874",
                "Most balanced mod only characters",
                "https://steamcommunity.com/app/632360/discussions/0/4428814237729615874/",
                "Included among nicely balanced modded survivors.",
                "nicely balanced -> A",
                "General",
                "",
                "2024-08-26",
                0.85f
            );
            Seed(
                "survivor:diggerunearthed.diggercontent:minerbody",
                "steam.2024.miner.vanilla-quality",
                CommunityPowerSourceKind.GuideOrReview,
                CommunityPowerTier.A,
                "steam:632360:6546631477883948372",
                "No op survivors mods",
                "https://steamcommunity.com/app/632360/discussions/0/6546631477883948372/",
                "EnforcerGang characters, explicitly including Miner, are recommended in a no-OP request as vanilla-quality.",
                "vanilla-quality in no-OP context -> A",
                "General",
                "",
                "2024-08-17",
                0.75f
            );
            Seed(
                "survivor:diggerunearthed.diggercontent:minerbody",
                "reddit.2022.miner.balanced-preference",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.A,
                "reddit:riskofrain:uii2na",
                "Good Playable Character Mods",
                "https://www.reddit.com/r/riskofrain/comments/uii2na/",
                "Players asking specifically for balanced/fun survivors list Miner among the characters they tried and loved.",
                "positive in explicit balanced-survivor context -> A; older/implicit evidence reduced",
                "General",
                "",
                "2022-05-04",
                0.60f
            );

            // HUNK --------------------------------------------------------
            Seed(
                "survivor:com.rob.hunk:robhunkbody",
                "steam.2024.hunk.balanced",
                CommunityPowerSourceKind.GuideOrReview,
                CommunityPowerTier.A,
                "steam:632360:4428814237729615874",
                "Most balanced mod only characters",
                "https://steamcommunity.com/app/632360/discussions/0/4428814237729615874/",
                "Included among nicely balanced modded survivors.",
                "nicely balanced -> A",
                "General",
                "",
                "2024-08-26",
                0.80f
            );
            Seed(
                "survivor:com.rob.hunk:robhunkbody",
                "reddit.2024.hunk.slightly-strong",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.A,
                "reddit:riskofrain:1gu91gw",
                "Give me any mods you know and I will play",
                "https://www.reddit.com/r/riskofrain/comments/1gu91gw/",
                "HUNK/Rob survivor is described as a little strong but not obnoxiously so, with low AoE offsetting it.",
                "slightly strong but explicitly offset/balanced -> A",
                "General",
                "",
                "2024-11-18",
                0.70f
            );

            // Nemesis Enforcer -------------------------------------------
            Seed(
                "survivor:enforcer.enforcercontent:nemesisenforcerbody",
                "reddit.2022.nemesis-enforcer.bit-op",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.S,
                "reddit:riskofrain:uby270",
                "I think Nemesis Enforcer is a bit OP",
                "https://www.reddit.com/r/riskofrain/comments/uby270/",
                "Explicitly described as a bit OP.",
                "bit OP -> S; old/version-unknown evidence reduced",
                "General",
                "",
                "2022-04-25",
                0.65f
            );
            Seed(
                "survivor:enforcer.enforcercontent:nemesisenforcerbody",
                "reddit.2021.nemesis-enforcer.broken",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.SPlus,
                "reddit:ror2:prchb8",
                "was a funny sight",
                "https://www.reddit.com/r/ror2/comments/prchb8/",
                "Player explicitly calls Nemesis Enforcer 100% broken but fun.",
                "explicit broken -> S+; very old/version-unknown evidence reduced",
                "General",
                "",
                "2021-09-20",
                0.55f
            );
            Seed(
                "survivor:enforcer.enforcercontent:nemesisenforcerbody",
                "reddit.2022.nemesis-enforcer.balanced-preference",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.A,
                "reddit:riskofrain:uii2na",
                "Good Playable Character Mods",
                "https://www.reddit.com/r/riskofrain/comments/uii2na/",
                "Players seeking balanced/fun characters include Enforcer + Nemesis Enforcer among the ones they like.",
                "positive in explicit balanced-survivor context -> A; indirect/old evidence strongly reduced",
                "General",
                "",
                "2022-05-04",
                0.45f
            );

            // Ralsei ------------------------------------------------------
            Seed(
                "survivor:com.godrayprod.ralseimod:ralseibody",
                "reddit.2025.ralsei.balanced",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.A,
                "reddit:riskofrain:1ns50d8",
                "What's your favourite moded survivor?",
                "https://www.reddit.com/r/riskofrain/comments/1ns50d8/",
                "Ralsei is explicitly praised for a fun, unique and balanced playstyle.",
                "balanced -> A",
                "General",
                "",
                "2025-09-27",
                0.90f
            );
            Seed(
                "survivor:com.godrayprod.ralseimod:ralseibody",
                "reddit.2026.ralsei.balance-caveat",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.A,
                "reddit:riskofrain:1sr3l7o",
                "Best Modded Survivors",
                "https://www.reddit.com/r/riskofrain/comments/1sr3l7o/",
                "In a request for fun-but-balanced survivors, Ralsei is recommended with the caveat that he is 'a little less so'.",
                "balanced-context recommendation with ambiguity -> A at low weight",
                "General",
                "",
                "2026-04-20",
                0.40f
            );

            // Evidencia todavía insuficiente: se conserva para auditoría
            // pero TryGet seguirá devolviendo Unknown (<2 grupos).

            // Aurelion Sol: un solo grupo independiente y además versión no
            // especificada; no debe poder imponer un CommunityPowerTier.
            Seed(
                "survivor:com.dragonyck.aurelionsol:aurelionsolbody",
                "reddit.2025.aurelion.weak-early",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.B,
                "reddit:riskofrain2_mods:1m9g97a",
                "Which is the WORST modded survivor you have ever tried?",
                "https://www.reddit.com/r/RiskofRain2_Mods/comments/1m9g97a/",
                "Aurelion is described as very weak early because his power is gated behind Stardust scaling.",
                "weak / below-reference early game -> B; single-source only",
                "Early game",
                "",
                "2025-07-26",
                0.60f
            );

            // Scout: sólo una discusión independiente verificable de balance.
            Seed(
                "survivor:com.kenko.scout:scoutbody",
                "steam.2024.scout.balanced",
                CommunityPowerSourceKind.GuideOrReview,
                CommunityPowerTier.A,
                "steam:632360:4428814237729615874",
                "Most balanced mod only characters",
                "https://steamcommunity.com/app/632360/discussions/0/4428814237729615874/",
                "Included among nicely balanced modded survivors.",
                "nicely balanced -> A; single independent group",
                "General",
                "",
                "2024-08-26",
                0.80f
            );

            // Wooper: dos opiniones del mismo hilo NO equivalen a dos
            // comunidades independientes.
            Seed(
                "survivor:com.dragonyck.wooper:wooperbody",
                "reddit.2026.wooper.too-strong",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.S,
                "reddit:riskofrain:1u9i989",
                "This was the best decision of my life",
                "https://www.reddit.com/r/riskofrain/comments/1u9i989/",
                "One player calls Wooper a bit too strong.",
                "too strong -> S; one independent thread",
                "General",
                "",
                "2026-06-18",
                0.75f
            );
            Seed(
                "survivor:com.dragonyck.wooper:wooperbody",
                "reddit.2026.wooper.overpowered",
                CommunityPowerSourceKind.Reddit,
                CommunityPowerTier.SPlus,
                "reddit:riskofrain:1u9i989",
                "This was the best decision of my life",
                "https://www.reddit.com/r/riskofrain/comments/1u9i989/",
                "A second comment in the same thread calls Wooper overpowered.",
                "explicit overpowered -> S+; same group, so not independent",
                "General",
                "",
                "2026-06-18",
                0.65f
            );
        }


        private void Seed(
            string survivorId,
            string sourceId,
            CommunityPowerSourceKind kind,
            CommunityPowerTier tier,
            string independentGroupId,
            string sourceTitle,
            string sourceReference,
            string evidenceLabel,
            string normalizationBasis,
            string context,
            string observedVersion,
            string publishedDate,
            float weight
        )
        {
            Register(
                new CommunityPowerSource
                {
                    SurvivorId = survivorId,
                    SourceId = sourceId,
                    Kind = kind,
                    Tier = tier,
                    IndependentGroupId = independentGroupId,
                    SourceTitle = sourceTitle,
                    SourceReference = sourceReference,
                    EvidenceLabel = evidenceLabel,
                    NormalizationBasis = normalizationBasis,
                    Context = context,
                    ObservedVersion = observedVersion,
                    PublishedDate = publishedDate,
                    Weight = weight,
                    IsPowerFocused = true
                }
            );
        }


        private static float Clamp(
            float value,
            float minimum,
            float maximum
        )
        {
            if (value < minimum)
            {
                return minimum;
            }

            if (value > maximum)
            {
                return maximum;
            }

            return value;
        }


        private sealed class GroupAccumulator
        {
            public float WeightedTier;
            public float TotalWeight;
            public int SourceCount;
        }


        private sealed class GroupScore
        {
            public float Tier;
            public float Weight;
        }
    }
}
