using System;
using System.Text;
using System.Text.RegularExpressions;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Presentación de diálogos ya escritos dentro de diarios USU.
    /// Solo separa visualmente intervenciones con comillas que parecen habla.
    /// No traduce, omite, inventa ni cambia letras del texto original.
    /// Nunca modifica una cita si no hay indicios de que sea una voz.
    /// Los límites de párrafo (\n\n / \r\n\r\n) se conservan.
    /// </summary>
    internal static class LoreDialogueLineFormatter
    {
        private static readonly Regex Quotes = new Regex(
            "«[^»\\r\\n]{2,180}»|“[^”\\r\\n]{2,180}”|\\\"[^\\\"\\r\\n]{2,180}\\\"",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex Attribution = new Regex(
            @"\b(dijo|preguntó|respondió|gritó|susurró|murmuró|exclamó|advirtió|contestó|declaró|digo|dije|shouted|said|asked|replied|whispered|yelled|called|answered|told|exclaimed|says|voice|voz|radio|comunicador|grabación|transmisión|gimió|pronunció|añadió)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal static string Format(string original)
        {
            if (string.IsNullOrEmpty(original)) return original ?? "";
            MatchCollection matches = Quotes.Matches(original);
            if (matches.Count == 0) return original;
            var output = new StringBuilder(original.Length + matches.Count * 2);
            int cursor = 0;
            foreach (Match match in matches)
            {
                if (match.Index < cursor) continue;
                int begin = Math.Max(0, match.Index - 95);
                int end = Math.Min(original.Length, match.Index + match.Length + 95);
                string surroundings = original.Substring(begin, end - begin);
                bool speech = Attribution.IsMatch(surroundings) ||
                    match.Value.IndexOf('!') >= 0 || match.Value.IndexOf('¡') >= 0 ||
                    match.Value.IndexOf('?') >= 0 || match.Value.IndexOf('¿') >= 0 ||
                    (match.Value.Length >= 18 && match.Value.IndexOf('.') >= 0);
                if (!speech) continue;
                output.Append(original, cursor, match.Index - cursor);
                if (output.Length > 0 && output[output.Length - 1] != '\n') output.Append('\n');
                output.Append(match.Value);
                if (match.Index + match.Length < original.Length &&
                    original[match.Index + match.Length] != '\n' &&
                    original[match.Index + match.Length] != '\r') output.Append('\n');
                cursor = match.Index + match.Length;
            }
            output.Append(original, cursor, original.Length - cursor);
            return output.ToString();
        }
    }
}
