using System;
using R2API;

namespace UniversalSurvivorUnlocks
{
    public static class SurvivorLocalizationKeys
    {
        public const string Sora = "official.sora";
        public const string Ralsei = "official.ralsei";
        public const string Jhin = "official.jhin";
        public const string Scout = "official.scout";
        public const string Spy = "official.spy";
        public const string Rocket = "official.rocket";
        public const string Hunk = "official.hunk";
        public const string Tinkaton = "official.tinkaton";
        public const string Wooper = "official.wooper";
    }


    /// <summary>
    /// Localización integrada de los presets oficiales de USU.
    /// RoR2 elige automáticamente el idioma actual al resolver los tokens.
    /// </summary>
    public static class SurvivorLocalization
    {
        private static bool initialized;


        private sealed class OfficialText
        {
            public string BodyName;
            public string LocalizationKey;
            public string SpanishName;
            public string SpanishDescription;
            public string EnglishName;
            public string EnglishDescription;
        }


        private static readonly OfficialText[] OfficialTexts =
        {
            new OfficialText
            {
                BodyName = "SoraBody",
                LocalizationKey = SurvivorLocalizationKeys.Sora,
                SpanishName = "Elegido de la Llave Espada",
                SpanishDescription =
                    "Abre paso entre mundos en Baluarte de Ambry;\n" +
                    "vence a sombras y completa Venganza - Mercenary",
                EnglishName = "Chosen of the Keyblade",
                EnglishDescription =
                    "Cross worlds through Bulwark's Ambry;\n" +
                    "defeat shadows; complete Vengeance - Mercenary"
            },

            new OfficialText
            {
                BodyName = "RalseiBody",
                LocalizationKey = SurvivorLocalizationKeys.Ralsei,
                SpanishName = "El poder de la bondad",
                SpanishDescription =
                    "Usa Devoción y reúne 3 nuevos amigos Lemurianos;\n" +
                    "completa el portal con ellos - Captain o Seeker",
                EnglishName = "The Power of Kindness",
                EnglishDescription =
                    "Use Devotion; recruit 3 Lemurian friends;\n" +
                    "complete the teleporter - Captain or Seeker"
            },

            new OfficialText
            {
                BodyName = "JhinBody",
                LocalizationKey = SurvivorLocalizationKeys.Jhin,
                SpanishName = "El Cuarto Acto",
                SpanishDescription =
                    "Convierte a un jefe en tu gran final;\n" +
                    "asesta un crítico mortal de 44.444 de daño o más.",
                EnglishName = "The Fourth Act",
                EnglishDescription =
                    "Turn a boss into your grand finale;\n" +
                    "land a lethal critical hit for 44,444+ damage."
            },

            new OfficialText
            {
                BodyName = "ScoutBody",
                LocalizationKey = SurvivorLocalizationKeys.Scout,
                SpanishName = "Sed Termonuclear",
                SpanishDescription =
                    "Sacia tu sed con 8 Bebidas energéticas;\n" +
                    "o completa el primer sector sin objetos en 4 min.",
                EnglishName = "Thermonuclear Thirst",
                EnglishDescription =
                    "Quench your thirst with 8 Energy Drinks;\n" +
                    "or clear the first stage itemless in 4 min."
            },

            new OfficialText
            {
                BodyName = "SpyBody",
                LocalizationKey = SurvivorLocalizationKeys.Spy,
                SpanishName = "Sin que me veas venir",
                SpanishDescription =
                    "Que el jefe nunca vea venir tu golpe final;\n" +
                    "remátalo por detrás con Daga serrada - Bandit",
                EnglishName = "Never Saw Me Coming",
                EnglishDescription =
                    "Make sure the boss never sees the final blow;\n" +
                    "backstab the boss with Serrated Dagger - Bandit"
            },

            new OfficialText
            {
                BodyName = "RocketSurvivorBody",
                LocalizationKey = SurvivorLocalizationKeys.Rocket,
                SpanishName = "La gravedad es opcional",
                SpanishDescription =
                    "Haz llover explosiones desde el cielo;\n" +
                    "derriba 5 antes de caer; haz la hazaña 3 veces.",
                EnglishName = "Gravity Is Optional",
                EnglishDescription =
                    "Rain explosions down from the sky;\n" +
                    "drop 5 before landing; pull it off 3 times."
            },

            new OfficialText
            {
                BodyName = "RobHunkBody",
                LocalizationKey = SurvivorLocalizationKeys.Hunk,
                SpanishName = "La Parca No Falla",
                SpanishDescription =
                    "Protege la batería y sobrevive a toda costa;\n" +
                    "escapa de la Luna o sacrifícate en el Obelisco.",
                EnglishName = "The Reaper Never Fails",
                EnglishDescription =
                    "Protect the Fuel Array; survive at all costs;\n" +
                    "escape the Moon or sacrifice at the Obelisk."
            },

            new OfficialText
            {
                BodyName = "TinkatonBody",
                LocalizationKey = SurvivorLocalizationKeys.Tinkaton,
                SpanishName = "Forjada en Chatarra",
                SpanishDescription =
                    "Haz de 6 chatarras el inicio de tu gran golpe;\n" +
                    "ten Justicia demoledora y vence un Ojo mecánico.",
                EnglishName = "Forged in Scrap",
                EnglishDescription =
                    "Scrap 6 items to prepare your crushing blow;\n" +
                    "hold Shattering Justice; defeat a mech Eye."
            },

            new OfficialText
            {
                BodyName = "WooperBody",
                LocalizationKey = SurvivorLocalizationKeys.Wooper,
                SpanishName = "De vuelta al agua",
                SpanishDescription =
                    "Haz de los Humedales tu hogar; marca territorio;\n" +
                    "caza y muerde a 20 presas envenenadas - Acrid",
                EnglishName = "Back to the Water",
                EnglishDescription =
                    "Make the Wetlands your home; mark your territory;\n" +
                    "hunt and bite 20 poisoned prey - Acrid"
            }
        };


        public static void EnsureRegistered()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            for (int i = 0; i < OfficialTexts.Length; i++)
            {
                RegisterOfficialText(OfficialTexts[i]);
            }
        }


        public static bool UsesBuiltInLocalization(
            string bodyName,
            SurvivorChallengeJson challenge
        )
        {
            if (
                string.IsNullOrWhiteSpace(bodyName) ||
                challenge == null ||
                string.IsNullOrWhiteSpace(challenge.LocalizationKey)
            )
            {
                return false;
            }

            for (int i = 0; i < OfficialTexts.Length; i++)
            {
                OfficialText text = OfficialTexts[i];

                if (
                    string.Equals(
                        text.BodyName,
                        bodyName,
                        StringComparison.Ordinal
                    ) &&
                    string.Equals(
                        text.LocalizationKey,
                        challenge.LocalizationKey,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return true;
                }
            }

            return false;
        }


        public static string GetOfficialTokenPrefix(string bodyName)
        {
            return $"USU_{MakeToken(bodyName)}_OFFICIAL";
        }


        public static string GetCustomTokenPrefix(string bodyName)
        {
            return $"USU_{MakeToken(bodyName)}_CUSTOM";
        }


        private static void RegisterOfficialText(OfficialText text)
        {
            if (text == null)
            {
                return;
            }

            string tokenPrefix =
                GetOfficialTokenPrefix(text.BodyName);

            string nameToken =
                $"{tokenPrefix}_ACHIEVEMENT_NAME";

            string descriptionToken =
                $"{tokenPrefix}_ACHIEVEMENT_DESCRIPTION";

            // Inglés = fallback para cualquier idioma todavía no traducido.
            LanguageAPI.Add(nameToken, text.EnglishName);
            LanguageAPI.Add(descriptionToken, text.EnglishDescription);

            LanguageAPI.Add(nameToken, text.EnglishName, "en");
            LanguageAPI.Add(descriptionToken, text.EnglishDescription, "en");

            RegisterSpanish(nameToken, text.SpanishName);
            RegisterSpanish(descriptionToken, text.SpanishDescription);
        }


        private static void RegisterSpanish(
            string token,
            string value
        )
        {
            LanguageAPI.Add(token, value, "es-419");
            LanguageAPI.Add(token, value, "es-ES");
        }


        private static string MakeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "UNKNOWN";
            }

            return value
                .Replace(".", "_")
                .Replace("-", "_")
                .Replace(" ", "_")
                .ToUpperInvariant();
        }
    }
}
