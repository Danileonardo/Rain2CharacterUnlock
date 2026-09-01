using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * PROGRESO DE UN JUGADOR DURANTE LA RUN
     * =============================================================
     *
     * No conserva referencias Unity/RoR2.
     *
     * Por eso una muerte o desconexión NO destruye
     * automáticamente el progreso.
     * =============================================================
     */
    public sealed class MissionPlayerProgress
    {
        public MissionPlayerIdentity Identity
        {
            get;
            private set;
        }


        public Dictionary<
            string,
            MissionProgressState
        > Missions
        {
            get;
            private set;
        }


        public MissionPlayerProgress(
            MissionPlayerIdentity identity
        )
        {
            Identity =
                identity != null
                    ? identity.Clone()
                    : null;


            Missions =
                new Dictionary<
                    string,
                    MissionProgressState
                >();
        }


        public void UpdateIdentity(
            MissionPlayerIdentity identity
        )
        {
            if (identity == null)
            {
                return;
            }


            Identity =
                identity.Clone();
        }


        public MissionProgressState
            GetOrCreateMission(
                string missionId
            )
        {
            if (
                string.IsNullOrWhiteSpace(
                    missionId
                )
            )
            {
                return null;
            }


            if (
                Missions.TryGetValue(
                    missionId,
                    out MissionProgressState state
                ) &&
                state != null
            )
            {
                return state;
            }


            state =
                new MissionProgressState(
                    missionId
                );


            Missions[
                missionId
            ] =
                state;


            return state;
        }


        public bool TryGetMission(
            string missionId,
            out MissionProgressState state
        )
        {
            state =
                null;


            if (
                string.IsNullOrWhiteSpace(
                    missionId
                )
            )
            {
                return false;
            }


            return Missions.TryGetValue(
                missionId,
                out state
            ) &&
            state != null;
        }


        public int ResetStageScopedObjectives()
        {
            int resetCount =
                0;


            foreach (
                MissionProgressState mission
                in Missions.Values
            )
            {
                if (mission == null)
                {
                    continue;
                }


                resetCount +=
                    mission
                        .ResetStageScopedObjectives();
            }


            return resetCount;
        }
    }
}
