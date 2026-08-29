using System.Collections.Generic;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public static class PlayerOwnerResolver
    {
        // =========================================================
        // RESOLVER JUGADOR PROPIETARIO
        // =========================================================
        //
        // Ejemplos:
        //
        // Jugador A
        //     -> A
        //
        // Missile Drone de A
        //     -> A
        //
        // Minion de un minion de A
        //     -> A
        //
        // Enemigo sin propietario jugador
        //     -> null
        //
        // =========================================================

        public static CharacterMaster ResolveOwningPlayerMaster(
            CharacterMaster sourceMaster
        )
        {
            if (sourceMaster == null)
            {
                return null;
            }


            HashSet<CharacterMaster> visited =
                new HashSet<CharacterMaster>();


            CharacterMaster currentMaster =
                sourceMaster;


            /*
             * El límite sólo existe como protección
             * contra una cadena corrupta/circular.
             *
             * En condiciones normales necesitaremos
             * muy pocos pasos.
             */
            const int MaximumOwnershipDepth =
                16;


            for (
                int depth = 0;
                depth < MaximumOwnershipDepth;
                depth++
            )
            {
                if (currentMaster == null)
                {
                    return null;
                }


                // =================================================
                // EVITAR CICLOS
                // =================================================

                if (
                    !visited.Add(
                        currentMaster
                    )
                )
                {
                    return null;
                }


                // =================================================
                // ¿ESTE MASTER ES UN JUGADOR REAL?
                // =================================================

                if (
                    IsPlayerMaster(
                        currentMaster
                    )
                )
                {
                    return currentMaster;
                }


                // =================================================
                // BUSCAR PROPIETARIO DEL MINION
                // =================================================

                MinionOwnership minionOwnership =
                    currentMaster.minionOwnership;


                if (minionOwnership == null)
                {
                    return null;
                }


                CharacterMaster ownerMaster =
                    minionOwnership.ownerMaster;


                if (ownerMaster == null)
                {
                    return null;
                }


                currentMaster =
                    ownerMaster;
            }


            return null;
        }


        // =========================================================
        // ¿ES MASTER DE UN JUGADOR?
        // =========================================================

        public static bool IsPlayerMaster(
            CharacterMaster master
        )
        {
            if (master == null)
            {
                return false;
            }


            foreach (
                PlayerCharacterMasterController controller
                in PlayerCharacterMasterController.instances
            )
            {
                if (controller == null)
                {
                    continue;
                }


                if (
                    controller.master ==
                    master
                )
                {
                    return true;
                }
            }


            return false;
        }
    }
}