using System;
using System.Collections;
using System.Reflection;
using System.Text;

using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Auditoría TEMPORAL y de sólo lectura para preparar los paquetes
    /// de localización curada posteriores a Aurelion Sol.
    ///
    /// No registra tokens, no cambia LanguageAPI, no altera unlocks y no
    /// modifica el Logbook. Sólo imprime exactamente lo que los Content
    /// Profiles ya resolvieron en runtime y los tokens nativos asociados.
    /// </summary>
    internal static class LocalizationCoverageDiagnostics
    {
        public static void Log(
            IEnumerable profiles,
            ManualLogSource logger
        )
        {
            if (profiles == null || logger == null)
            {
                return;
            }

            string language =
                Language.currentLanguageName ?? "";

            logger.LogInfo(
                "[LOCALIZATION AUDIT] ===== INICIO ===== | Idioma: " +
                language
            );

            int moddedCount = 0;
            int missingLoreCount = 0;
            int suspiciousLoreCount = 0;

            foreach (object profile in profiles)
            {
                if (profile == null || !IsModded(profile))
                {
                    continue;
                }

                moddedCount++;

                string bodyName = Text(profile, "BodyName");
                string displayName = Text(profile, "DisplayName");
                string source = Text(profile, "SourceIdentifier");
                string subtitle = Text(profile, "Subtitle");
                string description = Text(profile, "Description");
                string lore = Text(profile, "Lore");

                object survivorDef = Member(profile, "SurvivorDef");
                object bodyPrefab = Member(profile, "BodyPrefab");
                object characterBody =
                    InvokeComponent(bodyPrefab, "RoR2.CharacterBody");

                string nameToken = Text(survivorDef, "displayNameToken");
                string descriptionToken = Text(survivorDef, "descriptionToken");
                string subtitleToken =
                    FirstNonEmpty(
                        Text(characterBody, "subtitleNameToken"),
                        Text(characterBody, "subtitleToken")
                    );
                string loreToken =
                    FirstNonEmpty(
                        Text(survivorDef, "loreToken"),
                        Text(characterBody, "loreToken")
                    );

                bool missingLore = IsMissing(lore, loreToken);
                int loreWords = WordCount(lore);
                bool suspiciousLore =
                    !missingLore &&
                    (lore.Length < 120 || loreWords < 25);

                if (missingLore)
                {
                    missingLoreCount++;
                }
                else if (suspiciousLore)
                {
                    suspiciousLoreCount++;
                }

                logger.LogInfo(
                    "[LOCALIZATION AUDIT] SURVIVOR | Body: " + bodyName +
                    " | Name: " + Escape(displayName) +
                    " | Source: " + source
                );

                logger.LogInfo(
                    "[LOCALIZATION AUDIT] TOKENS | Body: " + bodyName +
                    " | NameToken: " + Escape(nameToken) +
                    " | SubtitleToken: " + Escape(subtitleToken) +
                    " | DescriptionToken: " + Escape(descriptionToken) +
                    " | LoreToken: " + Escape(loreToken)
                );

                logger.LogInfo(
                    "[LOCALIZATION AUDIT] SUBTITLE | Body: " + bodyName +
                    " | Text: " + Escape(subtitle)
                );

                logger.LogInfo(
                    "[LOCALIZATION AUDIT] DESCRIPTION | Body: " + bodyName +
                    " | Text: " + Escape(description)
                );

                logger.LogInfo(
                    "[LOCALIZATION AUDIT] LORE | Body: " + bodyName +
                    " | Missing: " + missingLore +
                    " | Suspicious: " + suspiciousLore +
                    " | Chars: " + lore.Length +
                    " | Words: " + loreWords +
                    " | Text: " + Escape(lore)
                );

                LogUnlock(
                    bodyName,
                    "SURVIVOR",
                    Member(profile, "SurvivorUnlock"),
                    logger
                );

                LogSkills(profile, bodyName, logger);
                LogSkins(profile, bodyName, logger);
            }

            logger.LogInfo(
                "[LOCALIZATION AUDIT] SUMMARY | Modded: " + moddedCount +
                " | Lore missing: " + missingLoreCount +
                " | Lore suspicious: " + suspiciousLoreCount
            );

            logger.LogInfo(
                "[LOCALIZATION AUDIT] ===== FIN ====="
            );
        }

        private static void LogSkills(
            object profile,
            string bodyName,
            ManualLogSource logger
        )
        {
            IEnumerable groups =
                Member(profile, "SkillGroups") as IEnumerable;

            if (groups == null)
            {
                return;
            }

            foreach (object group in groups)
            {
                if (group == null)
                {
                    continue;
                }

                string slot =
                    FirstNonEmpty(
                        Text(group, "SlotLabel"),
                        Text(group, "Slot")
                    );

                IEnumerable variants =
                    Member(group, "Variants") as IEnumerable;

                if (variants == null)
                {
                    continue;
                }

                foreach (object variant in variants)
                {
                    if (variant == null)
                    {
                        continue;
                    }

                    object skillDef = Member(variant, "SkillDef");

                    string internalName = Text(variant, "InternalName");
                    string displayName = Text(variant, "DisplayName");
                    string nameToken = Text(variant, "NameToken");
                    string descToken = Text(skillDef, "skillDescriptionToken");
                    string description = Text(variant, "Description");

                    if (
                        string.Equals(
                            slot,
                            "Passive",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        object passive =
                            ResolvePassiveSkill(profile);

                        if (string.IsNullOrWhiteSpace(nameToken))
                        {
                            nameToken =
                                FirstNonEmpty(
                                    Text(passive, "skillNameToken"),
                                    Text(passive, "nameToken")
                                );
                        }

                        if (string.IsNullOrWhiteSpace(descToken))
                        {
                            descToken =
                                FirstNonEmpty(
                                    Text(passive, "skillDescriptionToken"),
                                    Text(passive, "descriptionToken")
                                );
                        }
                    }

                    logger.LogInfo(
                        "[LOCALIZATION AUDIT] SKILL | Body: " + bodyName +
                        " | Slot: " + Escape(slot) +
                        " | Internal: " + Escape(internalName) +
                        " | NameToken: " + Escape(nameToken) +
                        " | Name: " + Escape(displayName) +
                        " | DescriptionToken: " + Escape(descToken) +
                        " | Description: " + Escape(description)
                    );

                    LogUnlock(
                        bodyName,
                        "SKILL " + internalName,
                        Member(variant, "Unlock"),
                        logger
                    );
                }
            }
        }

        private static object ResolvePassiveSkill(
            object profile
        )
        {
            object bodyPrefab =
                Member(
                    profile,
                    "BodyPrefab"
                );

            object skillLocator =
                InvokeComponent(
                    bodyPrefab,
                    "RoR2.SkillLocator"
                );

            return Member(
                skillLocator,
                "passiveSkill"
            );
        }

        private static void LogSkins(
            object profile,
            string bodyName,
            ManualLogSource logger
        )
        {
            IEnumerable skins =
                Member(profile, "Skins") as IEnumerable;

            if (skins == null)
            {
                return;
            }

            foreach (object skin in skins)
            {
                if (skin == null)
                {
                    continue;
                }

                string internalName = Text(skin, "InternalName");
                string displayName = Text(skin, "DisplayName");
                string nameToken = Text(skin, "NameToken");

                logger.LogInfo(
                    "[LOCALIZATION AUDIT] SKIN | Body: " + bodyName +
                    " | Internal: " + Escape(internalName) +
                    " | NameToken: " + Escape(nameToken) +
                    " | Name: " + Escape(displayName)
                );

                LogUnlock(
                    bodyName,
                    "SKIN " + internalName,
                    Member(skin, "Unlock"),
                    logger
                );
            }
        }

        private static void LogUnlock(
            string bodyName,
            string owner,
            object unlock,
            ManualLogSource logger
        )
        {
            if (unlock == null)
            {
                return;
            }

            string unlockable = Text(unlock, "UnlockableIdentifier");
            string achievement = Text(unlock, "AchievementIdentifier");
            string missionName = Text(unlock, "MissionName");
            string missionDescription = Text(unlock, "MissionDescription");

            if (
                string.IsNullOrWhiteSpace(unlockable) &&
                string.IsNullOrWhiteSpace(achievement) &&
                string.IsNullOrWhiteSpace(missionName) &&
                string.IsNullOrWhiteSpace(missionDescription)
            )
            {
                return;
            }

            string achievementNameToken = "";
            string achievementDescriptionToken = "";

            AchievementDef achievementDef =
                FindAchievement(achievement);

            if (achievementDef != null)
            {
                achievementNameToken = achievementDef.nameToken ?? "";
                achievementDescriptionToken =
                    achievementDef.descriptionToken ?? "";
            }

            logger.LogInfo(
                "[LOCALIZATION AUDIT] UNLOCK | Body: " + bodyName +
                " | Owner: " + Escape(owner) +
                " | Unlockable: " + Escape(unlockable) +
                " | Achievement: " + Escape(achievement) +
                " | NameToken: " + Escape(achievementNameToken) +
                " | Name: " + Escape(missionName) +
                " | DescriptionToken: " + Escape(achievementDescriptionToken) +
                " | Description: " + Escape(missionDescription)
            );
        }

        private static AchievementDef FindAchievement(
            string identifier
        )
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            try
            {
                Type type = typeof(AchievementManager);

                object definitions =
                    StaticMember(type, "achievementDefs");

                IEnumerable enumerable =
                    definitions as IEnumerable;

                if (enumerable == null)
                {
                    return null;
                }

                foreach (object raw in enumerable)
                {
                    AchievementDef def = raw as AchievementDef;

                    if (
                        def != null &&
                        string.Equals(
                            def.identifier,
                            identifier,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        return def;
                    }
                }
            }
            catch
            {
                // Diagnóstico: no debe afectar nunca la inicialización.
            }

            return null;
        }

        private static bool IsModded(object profile)
        {
            object survivorInfo =
                Member(profile, "SurvivorInfo");

            object value =
                Member(survivorInfo, "IsModded");

            return value is bool && (bool)value;
        }

        private static bool IsMissing(
            string text,
            string token
        )
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            string trimmed = text.Trim();

            if (
                !string.IsNullOrWhiteSpace(token) &&
                string.Equals(
                    trimmed,
                    token.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return true;
            }

            return false;
        }

        private static int WordCount(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            return text.Split(
                (char[])null,
                StringSplitOptions.RemoveEmptyEntries
            ).Length;
        }

        private static string Text(
            object instance,
            string memberName
        )
        {
            object value = Member(instance, memberName);
            return value != null ? value.ToString() : "";
        }

        private static object Member(
            object instance,
            string memberName
        )
        {
            if (instance == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            try
            {
                Type type = instance.GetType();

                PropertyInfo property =
                    type.GetProperty(
                        memberName,
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic
                    );

                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    return property.GetValue(instance, null);
                }

                FieldInfo field =
                    type.GetField(
                        memberName,
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic
                    );

                if (field != null)
                {
                    return field.GetValue(instance);
                }
            }
            catch
            {
                // Sólo auditoría.
            }

            return null;
        }

        private static object StaticMember(
            Type type,
            string memberName
        )
        {
            if (type == null)
            {
                return null;
            }

            PropertyInfo property =
                type.GetProperty(
                    memberName,
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

            if (property != null && property.GetIndexParameters().Length == 0)
            {
                return property.GetValue(null, null);
            }

            FieldInfo field =
                type.GetField(
                    memberName,
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

            return field != null
                ? field.GetValue(null)
                : null;
        }

        private static object InvokeComponent(
            object gameObject,
            string typeName
        )
        {
            if (gameObject == null)
            {
                return null;
            }

            try
            {
                Type componentType =
                    Type.GetType(typeName + ", RoR2");

                if (componentType == null)
                {
                    return null;
                }

                MethodInfo method =
                    gameObject.GetType().GetMethod(
                        "GetComponent",
                        new Type[] { typeof(Type) }
                    );

                if (method == null)
                {
                    return null;
                }

                return method.Invoke(
                    gameObject,
                    new object[] { componentType }
                );
            }
            catch
            {
                return null;
            }
        }

        private static string FirstNonEmpty(
            string first,
            string second
        )
        {
            return !string.IsNullOrWhiteSpace(first)
                ? first
                : second ?? "";
        }

        private static string Escape(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            StringBuilder builder = new StringBuilder(text.Length + 16);

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                switch (c)
                {
                    case '\r':
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            return builder.ToString().Trim();
        }
    }
}
