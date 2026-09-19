using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Coordina el UnlockableDef de USU, el estado Original y la aplicación
    /// runtime del provider. La implementación está dividida en archivos
    /// partial por responsabilidad para reducir acoplamiento visual sin
    /// modificar la lógica existente.
    /// </summary>
    public static partial class SurvivorUnlockManager
    {
        private static readonly Dictionary<string, UnlockableDef>
            CustomUnlockables =
                new Dictionary<string, UnlockableDef>();


        private static readonly Dictionary<string, AchievementDef>
            CustomAchievements =
                new Dictionary<string, AchievementDef>();


        private static readonly Dictionary<string, Sprite>
            CustomAchievementIcons =
                new Dictionary<string, Sprite>();


        private static readonly Dictionary<SurvivorDef, UnlockableDef>
            OriginalUnlockables =
                new Dictionary<SurvivorDef, UnlockableDef>();
    }
}
