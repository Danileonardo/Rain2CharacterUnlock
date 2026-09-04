using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public static class StatusEffectScanner
    {
        private static readonly Dictionary<BuffIndex, BuffDef>
            negativeBuffs =
                new Dictionary<BuffIndex, BuffDef>();

        private static readonly Dictionary<BuffIndex, BuffDef>
            positiveBuffs =
                new Dictionary<BuffIndex, BuffDef>();


        public static int Count
        {
            get
            {
                return negativeBuffs.Count;
            }
        }


        public static int NegativeCount
        {
            get
            {
                return negativeBuffs.Count;
            }
        }


        public static int PositiveCount
        {
            get
            {
                return positiveBuffs.Count;
            }
        }


        // =========================================================
        // RECONSTRUIR CATÁLOGO DE EFECTOS
        // =========================================================

        public static void Rebuild(
            ManualLogSource logger
        )
        {
            negativeBuffs.Clear();
            positiveBuffs.Clear();


            logger.LogInfo(
                "========== STATUS EFFECT SCANNER =========="
            );


            logger.LogInfo(
                $"Buffs registrados: {BuffCatalog.buffCount}"
            );


            for (
                int i = 0;
                i < BuffCatalog.buffCount;
                i++
            )
            {
                BuffIndex buffIndex =
                    (BuffIndex)i;


                BuffDef buffDef =
                    BuffCatalog.GetBuffDef(
                        buffIndex
                    );


                if (buffDef == null)
                {
                    continue;
                }


                /*
                 * Los DoT se cuentan directamente desde
                 * DotController.dotStackList.
                 *
                 * Por eso NO deben volver a contarse aquí
                 * mediante el BuffDef asociado.
                 */
                if (
                    buffDef.isDebuff &&
                    !buffDef.isDOT
                )
                {
                    negativeBuffs[
                        buffIndex
                    ] =
                        buffDef;

                    continue;
                }


                /*
                 * Candidato a estado beneficioso.
                 *
                 * Excluimos:
                 * - debuffs
                 * - cooldowns internos
                 * - buffs ocultos
                 * - indicadores de DoT
                 *
                 * No exigimos iconSprite para conservar
                 * compatibilidad con mods que crean buffs
                 * válidos sin icono.
                 */
                if (
                    !buffDef.isDebuff &&
                    !buffDef.isCooldown &&
                    !buffDef.isHidden &&
                    !buffDef.isDOT
                )
                {
                    positiveBuffs[
                        buffIndex
                    ] =
                        buffDef;
                }
            }


            logger.LogInfo(
                $"Debuffs de estado válidos: {negativeBuffs.Count}"
            );


            logger.LogInfo(
                $"Buffs positivos candidatos: {positiveBuffs.Count}"
            );


            logger.LogInfo(
                "==========================================="
            );
        }


        // =========================================================
        // SNAPSHOTS PARA EDITOR / PRESETS
        // =========================================================
        //
        // El editor futuro necesita poder listar exactamente los
        // BuffDef que el mismo scanner considera válidos.
        // Devolvemos copias para que nadie pueda modificar los
        // diccionarios internos del scanner.
        // =========================================================

        public static List<BuffDef> GetNegativeBuffsSnapshot()
        {
            return new List<BuffDef>(
                negativeBuffs.Values
            );
        }


        public static List<BuffDef> GetPositiveBuffsSnapshot()
        {
            return new List<BuffDef>(
                positiveBuffs.Values
            );
        }


        // =========================================================
        // NEGATIVOS
        // =========================================================

        public static bool IsNegative(
            BuffIndex buffIndex
        )
        {
            return
                negativeBuffs.ContainsKey(
                    buffIndex
                );
        }


        public static bool IsNegative(
            BuffDef buffDef
        )
        {
            if (buffDef == null)
            {
                return false;
            }


            return IsNegative(
                buffDef.buffIndex
            );
        }


        // =========================================================
        // POSITIVOS
        // =========================================================

        public static bool IsPositive(
            BuffIndex buffIndex
        )
        {
            return
                positiveBuffs.ContainsKey(
                    buffIndex
                );
        }


        public static bool IsPositive(
            BuffDef buffDef
        )
        {
            if (buffDef == null)
            {
                return false;
            }


            return IsPositive(
                buffDef.buffIndex
            );
        }
    }
}