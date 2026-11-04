using System;
using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public sealed class DarthVaderDefinition : SurvivorDefinition
    {
        private const string VaderSpanishOverview =
            "Darth Vader es un superviviente resistente que sacrifica movilidad a cambio de supervivencia.\n\n" +
            "<color=#CCD3E0>< ! > Las bonificaciones de velocidad de ataque y de movimiento no aumentan directamente esos atributos.\n\n" +
            "< ! > La velocidad de ataque adicional se convierte en daño, y la velocidad de movimiento adicional en armadura.\n\n" +
            "< ! > Ira desatada elimina los límites de ambas velocidades y permite aprovechar esas bonificaciones.\n\n" +
            "< ! > La Fuerza afecta a los objetivos situados en un cono frente a Vader.</color>";

        public override string SourceIdentifier => "com.PopcornFactory.DarthVaderMod";
        public override string BodyName => "DarthVaderBody";
        public override string DefinitionName => "Darth Vader";

        // Mod content-pack IDs vary independently of Thunderstore package IDs.
        // Identity remains restricted to this exact, distinctive BodyName.
        public override bool Matches(SurvivorInfo survivor)
        {
            return survivor != null && survivor.IsModded &&
                string.Equals(survivor.BodyName, BodyName, StringComparison.OrdinalIgnoreCase);
        }

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (!Matches(survivor)) return;

            // Diario nativo: mantener la integración previamente aprobada.
            NewSurvivorDiaryTokenResolver.Register(survivor, BodyName,
                DarthVaderLore.SpanishLore, DarthVaderLore.EnglishLore, logger);

            // Habilidades y skins reales del DarthVaderBody, sin tokens inventados
            // ni dependencias estáticas sobre la DLL de DarthVaderMod.
            DarthVaderSkillLocalization.RegisterFor(survivor, logger);
        }

        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            return (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase)
                ? VaderSpanishOverview : fallback ?? "";
        }

        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            string language = Language.currentLanguageName ?? "";
            if (language.StartsWith("es", StringComparison.OrdinalIgnoreCase))
                return DarthVaderLore.SpanishLore;
            if (language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                return DarthVaderLore.EnglishLore;
            return string.IsNullOrWhiteSpace(fallback) ? DarthVaderLore.EnglishLore : fallback;
        }
    }
}
