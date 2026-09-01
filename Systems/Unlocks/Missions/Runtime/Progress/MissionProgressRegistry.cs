using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * MISSION PROGRESS REGISTRY
     * =============================================================
     *
     * Fuente de verdad del PROGRESO durante una run.
     *
     * REGLAS:
     *
     * - El HOST/servidor es autoritativo.
     * - El cliente no puede modificar el registro directamente.
     * - No se guarda en disco.
     * - No depende de CharacterBody.
     * - No elimina progreso cuando desaparece NetworkUser.
     * - Todo se limpia al terminar la run.
     *
     * SOPORTA:
     *
     * - progreso Shared
     * - progreso PerPlayer
     * - objetivos Run
     * - objetivos Stage
     * - valor numérico
     * - flag Completed
     *
     * El reset automático al cambiar de sector se conectará
     * en el Paso 4 con el sistema genérico de stages.
     * =============================================================
     */
    public static class MissionProgressRegistry
    {
        private static ManualLogSource logger;

        private static bool initialized;

        private static bool runActive;


        /*
         * Progreso individual.
         *
         * KEY:
         *     MissionPlayerIdentity.Key
         *
         * VALUE:
         *     todas las misiones de ese jugador.
         */
        private static readonly Dictionary<
            string,
            MissionPlayerProgress
        > PlayerProgressByKey =
            new Dictionary<
                string,
                MissionPlayerProgress
            >();


        /*
         * Progreso compartido.
         *
         * KEY:
         *     MissionId / Body a desbloquear.
         */
        private static readonly Dictionary<
            string,
            MissionProgressState
        > SharedProgressByMission =
            new Dictionary<
                string,
                MissionProgressState
            >();


        public static bool IsRunActive =>
            runActive;


        public static int TrackedPlayerCount =>
            PlayerProgressByKey.Count;


        public static int SharedMissionCount =>
            SharedProgressByMission.Count;


        // =========================================================
        // INICIALIZAR
        // =========================================================

        public static void Initialize(
            ManualLogSource log
        )
        {
            if (initialized)
            {
                return;
            }


            initialized =
                true;

            logger =
                log;


            /*
             * Utilizamos exactamente la misma familia de eventos
             * Run que ya usa SessionMissionRegistry y que está
             * validada en el Paso 2.
             */
            Run.onRunStartGlobal +=
                OnRunStartGlobal;

            Run.onRunDestroyGlobal +=
                OnRunDestroyGlobal;


            logger?.LogInfo(
                "[MISSION PROGRESS] Registro de progreso inicializado."
            );
        }


        // =========================================================
        // RUN START
        // =========================================================

        private static void OnRunStartGlobal(
            Run run
        )
        {
            if (!NetworkServer.active)
            {
                return;
            }


            ClearInternal();

            runActive =
                true;


            logger?.LogInfo(
                "[MISSION PROGRESS] Nueva run | " +
                "progreso preparado en el host."
            );
        }


        // =========================================================
        // RUN END
        // =========================================================

        private static void OnRunDestroyGlobal(
            Run run
        )
        {
            if (!NetworkServer.active)
            {
                return;
            }


            int players =
                PlayerProgressByKey.Count;

            int sharedMissions =
                SharedProgressByMission.Count;


            ClearInternal();

            runActive =
                false;


            logger?.LogInfo(
                "[MISSION PROGRESS] Fin de run | " +
                $"Jugadores eliminados: {players} | " +
                $"Misiones shared eliminadas: {sharedMissions}"
            );
        }


        // =========================================================
        // STAGE
        // =========================================================

        /*
         * El Paso 4 llamará a este método cuando el tracker
         * genérico detecte correctamente el comienzo de un sector.
         *
         * Se deja público desde ahora para que todos los futuros
         * tipos de misión utilicen una sola política de reset.
         */
        public static void NotifyStageStarted()
        {
            if (!CanWrite())
            {
                return;
            }


            int resetCount =
                0;


            foreach (
                MissionPlayerProgress player
                in PlayerProgressByKey.Values
            )
            {
                if (player == null)
                {
                    continue;
                }


                resetCount +=
                    player
                        .ResetStageScopedObjectives();
            }


            foreach (
                MissionProgressState mission
                in SharedProgressByMission.Values
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


            logger?.LogInfo(
                "[MISSION PROGRESS] Nuevo sector | " +
                $"Objetivos Stage reiniciados: {resetCount}"
            );
        }


        // =========================================================
        // ADD PROGRESS
        // =========================================================

        public static double AddProgress(
            MissionProgressScope progressScope,
            NetworkUser networkUser,
            string missionId,
            string objectiveId,
            double amount,
            MissionProgressResetScope resetScope
        )
        {
            MissionObjectiveProgressState objective =
                GetOrCreateObjectiveForWrite(
                    progressScope,
                    networkUser,
                    missionId,
                    objectiveId,
                    resetScope
                );


            if (objective == null)
            {
                return 0d;
            }


            return objective.Add(
                amount
            );
        }


        public static double AddProgress(
            MissionProgressScope progressScope,
            CharacterMaster master,
            string missionId,
            string objectiveId,
            double amount,
            MissionProgressResetScope resetScope
        )
        {
            NetworkUser networkUser =
                MissionPlayerIdentity
                    .GetNetworkUser(
                        master
                    );


            return AddProgress(
                progressScope,
                networkUser,
                missionId,
                objectiveId,
                amount,
                resetScope
            );
        }


        public static double AddProgress(
            MissionProgressScope progressScope,
            CharacterBody body,
            string missionId,
            string objectiveId,
            double amount,
            MissionProgressResetScope resetScope
        )
        {
            CharacterMaster master =
                body != null
                    ? body.master
                    : null;


            return AddProgress(
                progressScope,
                master,
                missionId,
                objectiveId,
                amount,
                resetScope
            );
        }


        // =========================================================
        // SET PROGRESS
        // =========================================================

        public static double SetProgress(
            MissionProgressScope progressScope,
            NetworkUser networkUser,
            string missionId,
            string objectiveId,
            double value,
            MissionProgressResetScope resetScope
        )
        {
            MissionObjectiveProgressState objective =
                GetOrCreateObjectiveForWrite(
                    progressScope,
                    networkUser,
                    missionId,
                    objectiveId,
                    resetScope
                );


            if (objective == null)
            {
                return 0d;
            }


            return objective.SetValue(
                value
            );
        }


        // =========================================================
        // GET PROGRESS
        // =========================================================

        public static double GetProgress(
            MissionProgressScope progressScope,
            NetworkUser networkUser,
            string missionId,
            string objectiveId
        )
        {
            MissionObjectiveProgressState objective =
                GetObjectiveForRead(
                    progressScope,
                    networkUser,
                    missionId,
                    objectiveId
                );


            return
                objective != null
                    ? objective.Value
                    : 0d;
        }


        // =========================================================
        // COMPLETED FLAG DE OBJETIVO
        // =========================================================

        public static bool SetObjectiveCompleted(
            MissionProgressScope progressScope,
            NetworkUser networkUser,
            string missionId,
            string objectiveId,
            bool completed,
            MissionProgressResetScope resetScope
        )
        {
            MissionObjectiveProgressState objective =
                GetOrCreateObjectiveForWrite(
                    progressScope,
                    networkUser,
                    missionId,
                    objectiveId,
                    resetScope
                );


            if (objective == null)
            {
                return false;
            }


            objective.SetCompleted(
                completed
            );


            return objective.Completed;
        }


        public static bool IsObjectiveCompleted(
            MissionProgressScope progressScope,
            NetworkUser networkUser,
            string missionId,
            string objectiveId
        )
        {
            MissionObjectiveProgressState objective =
                GetObjectiveForRead(
                    progressScope,
                    networkUser,
                    missionId,
                    objectiveId
                );


            return
                objective != null &&
                objective.Completed;
        }


        // =========================================================
        // COMPLETED FLAG DE MISIÓN
        // =========================================================

        public static bool SetMissionCompleted(
            MissionProgressScope progressScope,
            NetworkUser networkUser,
            string missionId,
            bool completed
        )
        {
            MissionProgressState mission =
                GetOrCreateMissionForWrite(
                    progressScope,
                    networkUser,
                    missionId
                );


            if (mission == null)
            {
                return false;
            }


            mission.SetCompleted(
                completed
            );


            return mission.Completed;
        }


        public static bool IsMissionCompleted(
            MissionProgressScope progressScope,
            NetworkUser networkUser,
            string missionId
        )
        {
            MissionProgressState mission =
                GetMissionForRead(
                    progressScope,
                    networkUser,
                    missionId
                );


            return
                mission != null &&
                mission.Completed;
        }


        // =========================================================
        // RESET DE UN OBJETIVO
        // =========================================================

        public static bool ResetObjective(
            MissionProgressScope progressScope,
            NetworkUser networkUser,
            string missionId,
            string objectiveId
        )
        {
            MissionObjectiveProgressState objective =
                GetObjectiveForRead(
                    progressScope,
                    networkUser,
                    missionId,
                    objectiveId
                );


            if (objective == null)
            {
                return false;
            }


            objective.Reset();

            return true;
        }


        // =========================================================
        // RESET DE MISIÓN DE JUGADOR
        // =========================================================

        public static bool ResetPlayerMission(
            NetworkUser networkUser,
            string missionId
        )
        {
            if (
                !CanWrite() ||
                networkUser == null ||
                string.IsNullOrWhiteSpace(
                    missionId
                )
            )
            {
                return false;
            }


            MissionPlayerProgress player =
                FindExistingPlayer(
                    networkUser
                );


            if (player == null)
            {
                return false;
            }


            return player.Missions.Remove(
                missionId
            );
        }


        // =========================================================
        // IDENTIDAD / RECONEXIÓN
        // =========================================================

        public static bool TryGetPlayerIdentity(
            NetworkUser networkUser,
            out MissionPlayerIdentity identity
        )
        {
            return MissionPlayerIdentity
                .TryCreate(
                    networkUser,
                    out identity
                );
        }


        private static MissionPlayerProgress
            GetOrCreatePlayer(
                NetworkUser networkUser
            )
        {
            if (
                networkUser == null ||
                !MissionPlayerIdentity.TryCreate(
                    networkUser,
                    out MissionPlayerIdentity incomingIdentity
                ) ||
                incomingIdentity == null
            )
            {
                return null;
            }


            // ---------------------------------------------
            // 1. COINCIDENCIA EXACTA
            // ---------------------------------------------
            if (
                PlayerProgressByKey.TryGetValue(
                    incomingIdentity.Key,
                    out MissionPlayerProgress exact
                ) &&
                exact != null
            )
            {
                exact.UpdateIdentity(
                    incomingIdentity
                );


                return exact;
            }


            // ---------------------------------------------
            // 2. FALLBACK POR NICKNAME
            // ---------------------------------------------
            //
            // Sólo reutilizamos progreso si existe
            // UNA ÚNICA identidad compatible.
            //
            // Si ambas partes tienen StableId distintos,
            // NO mezclamos a dos jugadores con el mismo nombre.
            // ---------------------------------------------
            MissionPlayerProgress nicknameMatch =
                null;

            string normalizedIncomingNickname =
                MissionPlayerIdentity
                    .NormalizeNickname(
                        incomingIdentity.Nickname
                    );


            if (
                !string.IsNullOrWhiteSpace(
                    normalizedIncomingNickname
                )
            )
            {
                foreach (
                    MissionPlayerProgress candidate
                    in PlayerProgressByKey.Values
                )
                {
                    if (
                        candidate == null ||
                        candidate.Identity == null
                    )
                    {
                        continue;
                    }


                    string normalizedCandidateNickname =
                        MissionPlayerIdentity
                            .NormalizeNickname(
                                candidate
                                    .Identity
                                    .Nickname
                            );


                    if (
                        normalizedCandidateNickname !=
                            normalizedIncomingNickname
                    )
                    {
                        continue;
                    }


                    /*
                     * Dos IDs estables diferentes:
                     * son jugadores diferentes aunque compartan nombre.
                     */
                    if (
                        incomingIdentity.HasStableId &&
                        candidate.Identity.HasStableId &&
                        !string.Equals(
                            incomingIdentity.StableId,
                            candidate.Identity.StableId,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        continue;
                    }


                    /*
                     * Más de una coincidencia compatible:
                     * nickname ambiguo.
                     */
                    if (nicknameMatch != null)
                    {
                        nicknameMatch =
                            null;

                        break;
                    }


                    nicknameMatch =
                        candidate;
                }
            }


            if (nicknameMatch != null)
            {
                string oldKey =
                    nicknameMatch
                        .Identity
                        .Key;


                nicknameMatch.UpdateIdentity(
                    incomingIdentity
                );


                /*
                 * Si ahora conseguimos un StableId real,
                 * migramos la clave temporal basada en nickname
                 * a la nueva clave estable.
                 */
                if (
                    !string.Equals(
                        oldKey,
                        incomingIdentity.Key,
                        StringComparison.Ordinal
                    )
                )
                {
                    PlayerProgressByKey.Remove(
                        oldKey
                    );


                    PlayerProgressByKey[
                        incomingIdentity.Key
                    ] =
                        nicknameMatch;
                }


                logger?.LogInfo(
                    "[MISSION PROGRESS] Jugador recuperado | " +
                    $"Nickname: {incomingIdentity.Nickname} | " +
                    $"Identidad: {incomingIdentity.IdentitySource}"
                );


                return nicknameMatch;
            }


            // ---------------------------------------------
            // 3. JUGADOR NUEVO
            // ---------------------------------------------
            MissionPlayerProgress created =
                new MissionPlayerProgress(
                    incomingIdentity
                );


            PlayerProgressByKey[
                incomingIdentity.Key
            ] =
                created;


            logger?.LogInfo(
                "[MISSION PROGRESS] Jugador registrado | " +
                $"Nickname: {incomingIdentity.Nickname} | " +
                $"Identidad: {incomingIdentity.IdentitySource}"
            );


            return created;
        }


        private static MissionPlayerProgress
            FindExistingPlayer(
                NetworkUser networkUser
            )
        {
            if (
                networkUser == null ||
                !MissionPlayerIdentity.TryCreate(
                    networkUser,
                    out MissionPlayerIdentity identity
                ) ||
                identity == null
            )
            {
                return null;
            }


            if (
                PlayerProgressByKey.TryGetValue(
                    identity.Key,
                    out MissionPlayerProgress exact
                ) &&
                exact != null
            )
            {
                return exact;
            }


            /*
             * Para lectura también permitimos una recuperación
             * única por nickname.
             */
            MissionPlayerProgress match =
                null;


            string normalizedNickname =
                MissionPlayerIdentity
                    .NormalizeNickname(
                        identity.Nickname
                    );


            foreach (
                MissionPlayerProgress candidate
                in PlayerProgressByKey.Values
            )
            {
                if (
                    candidate == null ||
                    candidate.Identity == null
                )
                {
                    continue;
                }


                if (
                    MissionPlayerIdentity
                        .NormalizeNickname(
                            candidate
                                .Identity
                                .Nickname
                        ) !=
                    normalizedNickname
                )
                {
                    continue;
                }


                if (
                    identity.HasStableId &&
                    candidate.Identity.HasStableId &&
                    !string.Equals(
                        identity.StableId,
                        candidate.Identity.StableId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }


                if (match != null)
                {
                    return null;
                }


                match =
                    candidate;
            }


            return match;
        }


        // =========================================================
        // MISSION WRITE
        // =========================================================

        private static MissionProgressState
            GetOrCreateMissionForWrite(
                MissionProgressScope progressScope,
                NetworkUser networkUser,
                string missionId
            )
        {
            if (
                !CanWrite() ||
                string.IsNullOrWhiteSpace(
                    missionId
                )
            )
            {
                return null;
            }


            if (
                progressScope ==
                MissionProgressScope.Shared
            )
            {
                if (
                    SharedProgressByMission
                        .TryGetValue(
                            missionId,
                            out MissionProgressState shared
                        ) &&
                    shared != null
                )
                {
                    return shared;
                }


                shared =
                    new MissionProgressState(
                        missionId
                    );


                SharedProgressByMission[
                    missionId
                ] =
                    shared;


                return shared;
            }


            MissionPlayerProgress player =
                GetOrCreatePlayer(
                    networkUser
                );


            return player?
                .GetOrCreateMission(
                    missionId
                );
        }


        // =========================================================
        // MISSION READ
        // =========================================================

        private static MissionProgressState
            GetMissionForRead(
                MissionProgressScope progressScope,
                NetworkUser networkUser,
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
                progressScope ==
                MissionProgressScope.Shared
            )
            {
                SharedProgressByMission
                    .TryGetValue(
                        missionId,
                        out MissionProgressState shared
                    );


                return shared;
            }


            MissionPlayerProgress player =
                FindExistingPlayer(
                    networkUser
                );


            if (
                player == null ||
                !player.TryGetMission(
                    missionId,
                    out MissionProgressState mission
                )
            )
            {
                return null;
            }


            return mission;
        }


        // =========================================================
        // OBJECTIVE WRITE
        // =========================================================

        private static MissionObjectiveProgressState
            GetOrCreateObjectiveForWrite(
                MissionProgressScope progressScope,
                NetworkUser networkUser,
                string missionId,
                string objectiveId,
                MissionProgressResetScope resetScope
            )
        {
            if (
                string.IsNullOrWhiteSpace(
                    objectiveId
                )
            )
            {
                return null;
            }


            MissionProgressState mission =
                GetOrCreateMissionForWrite(
                    progressScope,
                    networkUser,
                    missionId
                );


            return mission?
                .GetOrCreateObjective(
                    objectiveId,
                    resetScope
                );
        }


        // =========================================================
        // OBJECTIVE READ
        // =========================================================

        private static MissionObjectiveProgressState
            GetObjectiveForRead(
                MissionProgressScope progressScope,
                NetworkUser networkUser,
                string missionId,
                string objectiveId
            )
        {
            MissionProgressState mission =
                GetMissionForRead(
                    progressScope,
                    networkUser,
                    missionId
                );


            if (
                mission == null ||
                !mission.TryGetObjective(
                    objectiveId,
                    out MissionObjectiveProgressState objective
                )
            )
            {
                return null;
            }


            return objective;
        }


        // =========================================================
        // AUTORIDAD
        // =========================================================

        private static bool CanWrite()
        {
            return
                NetworkServer.active &&
                runActive;
        }


        // =========================================================
        // LIMPIEZA
        // =========================================================

        private static void ClearInternal()
        {
            PlayerProgressByKey.Clear();

            SharedProgressByMission.Clear();
        }
    }
}
