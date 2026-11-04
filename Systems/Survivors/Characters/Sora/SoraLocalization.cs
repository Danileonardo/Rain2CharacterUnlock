using System;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Datos de descripción y localización propios de Sora.
    /// Extraídos sin alterar contenido ni el orden de sus textos.
    /// </summary>
    internal static class SoraLocalization
    {
        internal static readonly string SoraSpanishDescription =
            "Sora combina ataques rápidos con la Llave Espada, la versatilidad de la Magia y una gran movilidad aérea.<style=cSub>\r\n\r\n" +
            "< ! > Administra tus MP con cuidado; al agotarlos entrarás en Estado de recuperación y no podrás lanzar Magia temporalmente.<style=cSub>\r\n\r\n" +
            "< ! > Durante el Estado de recuperación, Ataque se vuelve mucho más agresivo y permite encadenar golpes con mayor facilidad.<style=cSub>\r\n\r\n" +
            "< ! > Usa Esquivar para reposicionarte rápidamente o mantén la habilidad para recorrer distancias mayores con Deslizarse.<style=cSub>\r\n\r\n" +
            "< ! > Ataque veloz puede atravesar grupos enteros y recuperar MP al eliminar enemigos.";

        internal static readonly string SoraEnglishDescription =
            "Sora combines swift Keyblade attacks, versatile Magic, and strong aerial mobility.<style=cSub>\r\n\r\n" +
            "< ! > Manage MP carefully; exhausting it puts Sora into Recovery State and temporarily prevents him from casting Magic.<style=cSub>\r\n\r\n" +
            "< ! > During Recovery State, Attack becomes much more aggressive and can chain hits together more freely.<style=cSub>\r\n\r\n" +
            "< ! > Tap Dodge to reposition quickly or hold the skill to cover longer distances with Slide.<style=cSub>\r\n\r\n" +
            "< ! > Strike Raid can pierce entire groups and recover MP when it kills enemies.";

        internal static readonly string SoraSpanishPassiveDescription =
            "La <color=#0172eb>Cadena del Reino</color> concede a Sora habilidades mágicas especiales." +
            Environment.NewLine +
            "Sora puede mantenerse suspendido en el aire de forma pasiva. Además, mantener pulsado Salto hace que Sora " +
            "<style=cIsUtility>se deslice hacia delante</style>." +
            Environment.NewLine +
            "Sora posee una <color=#0172eb>barra de MP</color> que le permite lanzar Magia a cambio de " +
            "<color=#0172eb>MP</color>. Cuando los <color=#0172eb>MP</color> llegan a 0, Sora entra en " +
            "<color=#e1279f>Estado de recuperación</color> durante <style=cIsUtility>20 s</style>; durante ese tiempo " +
            "no puede usar Magia y <style=cIsDamage>Ataque</style> queda considerablemente potenciado.";

    }
}
