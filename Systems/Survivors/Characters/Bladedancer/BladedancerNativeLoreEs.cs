using System;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Traduccion al espanol del REGISTRO NATIVO incluido en IreliaMod.dll
    /// (Slendykray / plnk). Solo se aplica en tiempo de ejecucion sobre el token
    /// del Logbook de Bladedancer; no modifica la DLL ni la version en ingles.
    /// Incluye terminal, poema y mensaje de cierre. La continuacion USU
    /// «Los nombres bajo la piedra» permanece en BladedancerLore.cs.
    /// </summary>
    internal static class BladedancerNativeLoreEs
    {
        // Evita reemplazar un lore distinto si el creador modifica el contenido.
        internal static bool MatchesKnownOriginal(string original)
        {
            return !string.IsNullOrWhiteSpace(original)
                && original.IndexOf("Elegy of the Blade Dancer", StringComparison.OrdinalIgnoreCase) >= 0
                && original.IndexOf("DIGITIZING ENTRY", StringComparison.OrdinalIgnoreCase) >= 0
                && original.IndexOf("MESSAGE SENT", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static readonly string Text = @"<style=cMono>>> PROCESANDO CONSULTA...
>> RESULTADOS OBTENIDOS: 0</style>

> searchmod archivalscan

<style=cMono>>> PROCESANDO CONSULTA: EXPLORACIÓN DE ARCHIVOS...
>> ESPERE...
>> RESULTADOS OBTENIDOS: 1 ENTRADA. ÚLTIMO ACCESO: HACE 12010 DÍAS</style>

> view

<style=cMono>>> DIGITALIZANDO ENTRADA...
>> MOSTRANDO RESULTADO:</style>

Elegía de la Danzarina de las Hojas

Una tierra donde la sangre riega los árboles recuerda
La historia que las voces ancestrales llevan por colinas, valles y mares
Vientos que susurran sobre <sub>una niña indefensa una insurgente un baluarte</sub> una heroína
La elegante Danzarina de las Hojas, que gira, danza y vuelve a girar

Donde las tumbas superan en número a granjas, molinos, pozos y colinas
Una tierra desgarrada, quemada y ensangrentada
Donde ella se alzó <sub>y danzó</sub> y luchó <sub>y danzó</sub> y triunfó
Donde campesinos, carpinteros, ganaderos, mendigos y niños
Se alzaron junto a ella en desafío

Los vientos se aquietan al acercarse su pareja de baile
Pasos pesados, brazos rígidos, mente cerrada
Los movimientos se tambalean —
    y se sacuden —
        y vacilan —
y se detienen. Un giro hábil, una vuelta y una torsión de cadera, hombro y brazo
    <sup>hoja tras hoja tras hoja</sup>
Terminan la danza demasiado pronto

Y así se mantuvo en pie la protectora de aquella tierra
Donde había perdido, encontrado, sangrado, matado y resistido
Hasta que dejó de estar allí
La danzarina no puede permanecer inmóvil
    <sup>no puede conformarse no puede ser rígida no puede estancarse</sup>
Esta desea bailar sola

<style=cMono>>> FIN DEL RESULTADO</style>

> composemsg to 'C'

<style=cMono>>> INTRODUCIENDO MENSAJE</style>

Oye, esto es todo lo que he podido encontrar. Los únicos registros relacionados que hallé hablan de un antiguo clan del este, aniquilado hace siglos. Pero sigue sin tener nada que ver con nuestra chica, así que no tengo nada más. Revisa tus otras fuentes y mira si puedes descubrir algo sobre ella que las bases de datos no nos muestren.

> send

<style=cMono>>> MENSAJE ENVIADO</style>";
    }
}
