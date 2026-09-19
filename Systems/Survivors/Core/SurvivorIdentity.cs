using System;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Identidad persistente mínima de un survivor.
    ///
    /// USU no serializa el perfil runtime del personaje cuando el mod no está
    /// instalado. La identidad sólo existe para volver a asociar su misión al
    /// contenido correcto cuando el Body reaparece.
    /// </summary>
    public static class SurvivorIdentity
    {
        private const string SourcePrefix = "survivor:";
        private const string BodyFallbackPrefix = "survivor-body:";

        public static string Build(
            string sourceIdentifier,
            string bodyName
        )
        {
            string body =
                NormalizePart(
                    bodyName
                );

            if (string.IsNullOrWhiteSpace(body))
            {
                return "";
            }

            string source =
                NormalizePart(
                    sourceIdentifier
                );

            if (string.IsNullOrWhiteSpace(source))
            {
                return
                    BodyFallbackPrefix +
                    body;
            }

            return
                SourcePrefix +
                source +
                ":" +
                body;
        }


        public static string Build(
            SurvivorInfo survivor
        )
        {
            if (survivor == null)
            {
                return "";
            }

            string source =
                !string.IsNullOrWhiteSpace(
                    survivor.ContentPackIdentifier
                )
                    ? survivor.ContentPackIdentifier
                    : survivor.SourceAssembly;

            return Build(
                source,
                survivor.BodyName
            );
        }


        public static bool IsBodyFallback(
            string survivorId
        )
        {
            return
                !string.IsNullOrWhiteSpace(
                    survivorId
                ) &&
                survivorId.StartsWith(
                    BodyFallbackPrefix,
                    StringComparison.OrdinalIgnoreCase
                );
        }


        public static bool Matches(
            string survivorId,
            string sourceIdentifier,
            string bodyName
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    survivorId
                )
            )
            {
                return false;
            }

            string expected =
                Build(
                    sourceIdentifier,
                    bodyName
                );

            return
                !string.IsNullOrWhiteSpace(
                    expected
                ) &&
                string.Equals(
                    survivorId,
                    expected,
                    StringComparison.OrdinalIgnoreCase
                );
        }


        private static string NormalizePart(
            string value
        )
        {
            return
                string.IsNullOrWhiteSpace(
                    value
                )
                    ? ""
                    : value.Trim()
                        .ToLowerInvariant();
        }
    }
}
