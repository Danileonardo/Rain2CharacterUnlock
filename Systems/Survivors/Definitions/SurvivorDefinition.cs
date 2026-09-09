using System;
using System.Reflection;

using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Base común para las integraciones curadas de survivors conocidas por USU.
    ///
    /// No referencia DLLs de mods externos. Cada Definition sólo se activa cuando
    /// el detector de USU encuentra el Body + ContentPack correspondiente.
    ///
    /// Fase actual:
    /// - identidad / matching;
    /// - localización curada y reparación visual;
    /// - lore / Logbook;
    /// - metadata visible del unlock original.
    ///
    /// Misiones oficiales USU, providers y CUSTOM se migrarán posteriormente sin
    /// alterar todavía los sistemas runtime existentes.
    /// </summary>
    public abstract class SurvivorDefinition
    {
        public abstract string SourceIdentifier
        {
            get;
        }

        public abstract string BodyName
        {
            get;
        }

        public virtual string DefinitionName
        {
            get
            {
                return BodyName;
            }
        }

        public virtual bool Matches(
            SurvivorInfo survivor
        )
        {
            if (
                survivor == null ||
                !survivor.IsModded
            )
            {
                return false;
            }

            return
                string.Equals(
                    survivor.BodyName,
                    BodyName,
                    System.StringComparison.OrdinalIgnoreCase
                ) &&
                string.Equals(
                    survivor.ContentPackIdentifier,
                    SourceIdentifier,
                    System.StringComparison.OrdinalIgnoreCase
                );
        }

        /// <summary>
        /// Registra únicamente localización nativa correspondiente a esta
        /// Definition activa. La implementación debe ser segura si el mod no
        /// aporta alguno de sus tokens/achievements.
        /// </summary>
        public virtual void RegisterInstalledLocalization(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
        }

        public virtual string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return fallback ?? "";
        }

        public virtual string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return fallback ?? "";
        }

        public virtual string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return fallback ?? "";
        }

        public virtual string ResolveOriginalMissionName(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return fallback ?? "";
        }

        public virtual string ResolveOriginalMissionDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return fallback ?? "";
        }

        /// <summary>
        /// Ajuste puramente visual para descripciones de habilidades largas.
        ///
        /// No resume, elimina ni reescribe contenido. Sólo antepone un tag de
        /// tamaño de TextMeshPro cuando el texto visible supera ciertos umbrales,
        /// para que las traducciones extensas aprovechen mejor el espacio que ya
        /// ofrece Character Select / Loadout. Los textos cortos quedan intactos.
        ///
        /// Se cuentan únicamente caracteres visibles: los tags de rich text no
        /// deben hacer que una descripción parezca artificialmente más larga.
        /// </summary>
        protected static string FormatLongSkillText(
            string text
        )
        {
            return FormatLongSkillText(
                text,
                false
            );
        }


        /// <summary>
        /// Variante del ajuste visual que puede reservar una pequeña separación
        /// vertical antes de la descripción. Se usa sólo cuando una integración
        /// concreta demuestra que la UI estándar deja el texto demasiado pegado
        /// al nombre de la habilidad.
        ///
        /// La separación se crea con una línea al 45% del tamaño normal, por lo
        /// que no añade una línea completa ni modifica ninguna palabra, mecánica,
        /// porcentaje o tag del texto original.
        /// </summary>
        protected static string FormatLongSkillText(
            string text,
            bool addLeadingSpacing
        )
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text ?? "";
            }

            int visibleCharacters =
                CountVisibleCharacters(text);

            int sizePercent =
                visibleCharacters >= 520
                    ? 82
                    : visibleCharacters >= 380
                        ? 86
                        : visibleCharacters >= 260
                            ? 90
                            : 100;

            string leadingSpacing =
                addLeadingSpacing
                    ? "<size=45%>\n</size>"
                    : "";

            if (sizePercent >= 100)
            {
                return leadingSpacing + text;
            }

            // Igual que muchos textos nativos de RoR2, dejamos el tag activo
            // hasta el final del token. No se altera ninguna palabra ni dato.
            return
                leadingSpacing +
                "<size=" +
                sizePercent +
                "%>" +
                text;
        }


        private static int CountVisibleCharacters(
            string text
        )
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int count = 0;
            bool insideTag = false;

            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];

                if (character == '<')
                {
                    insideTag = true;
                    continue;
                }

                if (insideTag)
                {
                    if (character == '>')
                    {
                        insideTag = false;
                    }

                    continue;
                }

                if (!char.IsWhiteSpace(character))
                {
                    count++;
                }
            }

            return count;
        }


        /// <summary>
        /// Obtiene los tokens reales de la pasiva declarada por el SkillLocator
        /// del survivor. Esto evita asumir que una pasiva es un SkillDef normal:
        /// RoR2 la expone mediante SkillLocator.passiveSkill.
        ///
        /// La Definition puede usar este helper para localizar sólo la pasiva
        /// que el mod creador realmente publica, sin inventar tokens.
        /// </summary>
        protected static bool TryGetPassiveLocalizationTokens(
            SurvivorInfo survivor,
            out string nameToken,
            out string descriptionToken
        )
        {
            nameToken = "";
            descriptionToken = "";

            if (
                survivor == null ||
                survivor.SurvivorDef == null ||
                survivor.SurvivorDef.bodyPrefab == null
            )
            {
                return false;
            }

            try
            {
                SkillLocator locator =
                    survivor.SurvivorDef.bodyPrefab
                        .GetComponent<SkillLocator>();

                if (locator == null)
                {
                    return false;
                }

                object passive =
                    ReadMemberObject(
                        locator,
                        "passiveSkill"
                    );

                if (passive == null)
                {
                    return false;
                }

                nameToken =
                    FirstNonEmpty(
                        ReadStringMember(
                            passive,
                            "skillNameToken"
                        ),
                        ReadStringMember(
                            passive,
                            "nameToken"
                        )
                    );

                descriptionToken =
                    FirstNonEmpty(
                        ReadStringMember(
                            passive,
                            "skillDescriptionToken"
                        ),
                        ReadStringMember(
                            passive,
                            "descriptionToken"
                        )
                    );

                return
                    !string.IsNullOrWhiteSpace(nameToken) ||
                    !string.IsNullOrWhiteSpace(descriptionToken);
            }
            catch
            {
                // Una Definition nunca debe impedir la carga del survivor.
                return false;
            }
        }

        private static object ReadMemberObject(
            object instance,
            string memberName
        )
        {
            if (
                instance == null ||
                string.IsNullOrWhiteSpace(memberName)
            )
            {
                return null;
            }

            Type type =
                instance.GetType();

            PropertyInfo property =
                type.GetProperty(
                    memberName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

            if (
                property != null &&
                property.GetIndexParameters().Length == 0
            )
            {
                return property.GetValue(
                    instance,
                    null
                );
            }

            FieldInfo field =
                type.GetField(
                    memberName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

            return field != null
                ? field.GetValue(instance)
                : null;
        }

        private static string ReadStringMember(
            object instance,
            string memberName
        )
        {
            object value =
                ReadMemberObject(
                    instance,
                    memberName
                );

            return value != null
                ? value.ToString()
                : "";
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
    }
}
