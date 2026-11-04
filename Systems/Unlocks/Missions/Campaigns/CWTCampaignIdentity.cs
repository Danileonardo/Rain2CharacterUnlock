using System;
namespace UniversalSurvivorUnlocks
{
    // No se adivina el identificador interno: se captura del SurvivorInfo real.
    // La implementación no referencia DLL del autor ni sustituye su lore.
    internal static class CWTCampaignIdentity
    {
        internal static string BodyName { get; private set; } = "TankBody";
        internal static string ContentPackIdentifier { get; private set; } = "";
        internal static bool Detected { get; private set; }
        internal static bool Observe(SurvivorInfo info)
        {
            if (info == null || !info.IsModded || string.IsNullOrWhiteSpace(info.BodyName)) return false;
            string display = (info.DisplayName ?? "").ToLowerInvariant();
            string pack = (info.ContentPackIdentifier ?? "").ToLowerInvariant();
            string src = (info.SourceAssembly ?? "").ToLowerInvariant();
            string internalName = (info.InternalName ?? "").ToLowerInvariant();
            // Excluir Mega Tank y los demás vehículos del mismo paquete.
            if (display.Contains("mega") || display.Contains("scout car") ||
                display.Contains("helicopter") || internalName.Contains("mega")) return false;
            bool exactDisplay = display.Contains("celestial war tank") ||
                (display.Contains("celestial") && display.Contains("tank"));
            bool producerAndTank = (pack.Contains("cheesewithholes") ||
                pack.Contains("celestial") || src.Contains("celestial")) &&
                (info.BodyName.IndexOf("tank", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 internalName.Contains("tank"));
            if (!exactDisplay && !producerAndTank) return false;
            BodyName = info.BodyName;
            ContentPackIdentifier = info.ContentPackIdentifier ?? "";
            Detected = !string.IsNullOrWhiteSpace(ContentPackIdentifier);
            return true;
        }
        internal static bool IsBody(string bodyName)
        {
            return Detected && string.Equals(bodyName, BodyName, StringComparison.OrdinalIgnoreCase);
        }
    }
}