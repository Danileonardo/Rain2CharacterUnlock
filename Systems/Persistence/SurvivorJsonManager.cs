using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public static partial class SurvivorJsonManager
    {
        public static SurvivorJsonFile CurrentConfig
        {
            get;
            private set;
        } = new SurvivorJsonFile();


        private static string ConfigDirectory =>
            System.IO.Path.Combine(
                BepInEx.Paths.ConfigPath,
                "UniversalSurvivorUnlocks"
            );


        private static string JsonPath =>
            System.IO.Path.Combine(
                ConfigDirectory,
                "Survivors.json"
            );


        private static string BackupPath =>
            System.IO.Path.Combine(
                ConfigDirectory,
                "Survivors.backup.json"
            );


        private static string TempPath =>
            System.IO.Path.Combine(
                ConfigDirectory,
                "Survivors.tmp.json"
            );

    }
}
