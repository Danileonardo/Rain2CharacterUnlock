using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UniversalSurvivorUnlocks
{
    public static class SurvivorLockedPortraitManager
    {
        private static bool initialized;

        private static ManualLogSource Logger;

        private static readonly Dictionary<
            SurvivorIconController,
            Color
        > OriginalColors =
            new Dictionary<
                SurvivorIconController,
                Color
            >();

        /*
         * Cuando un survivor cambia de provider durante esta sesión, USU
         * mantiene su visual bloqueado coherente incluso si el provider
         * actual vuelve a ser Original. La disponibilidad real la sigue
         * calculando RoR2 mediante survivorIsUnlocked; USU sólo asegura que
         * un estado bloqueado no quede accidentalmente a color después de un
         * hot-swap de UnlockableDef.
         */
        private static readonly HashSet<string> RuntimeManagedBodies =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase
            );

        /*
         * FIX6:
         * ScrollableLobbyUI reemplaza CharacterSelectBarController.Build y
         * reasigna survivorDef a sus celdas sin forzar SurvivorIconController
         * .Rebuild(). Por eso el provider/unlock real cambia, pero el tooltip
         * visible puede conservar los tokens anteriores hasta volver a entrar
         * al lobby.
         *
         * Guardamos un pequeño refresh diferido de respaldo para el siguiente
         * FixedUpdate. No es polling permanente: sólo vive unos pocos ticks
         * después de un cambio de provider.
         */
        private static string PendingUiRefreshBody = "";

        private static int PendingUiRefreshTicks = 0;

        private const int MaxPendingUiRefreshTicks = 4;

        public static void Initialize(
            ManualLogSource logger
        )
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            Logger = logger;

            On.RoR2.UI.SurvivorIconController.Rebuild +=
                SurvivorIconController_Rebuild;

            On.RoR2.UI.SurvivorIconController.UpdateAvailability +=
                SurvivorIconController_UpdateAvailability;

            RoR2Application.onFixedUpdate +=
                OnFixedUpdate;

            UsuLog.Verbose(
                Logger,
                "SurvivorLockedPortraitManager inicializado."
            );
        }

        /// <summary>
        /// Reconstruye la parte local del selector después de cambiar
        /// Original/USU/Custom en runtime.
        ///
        /// Importante:
        /// - Antes de reconstruir se eliminan únicamente los overrides de
        ///   color aplicados por USU.
        /// - RoR2 vuelve a calcular tooltip, disponibilidad y visuales usando
        ///   el UnlockableDef que esté activo en ese instante.
        /// - Después USU sólo vuelve a oscurecer los UnlockableDef propios que
        ///   continúen bloqueados.
        /// </summary>
        public static void RefreshAll(
            string bodyName = null
        )
        {
            string normalizedBodyName =
                bodyName?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(normalizedBodyName))
            {
                RuntimeManagedBodies.Add(
                    normalizedBodyName
                );
            }

            /*
             * Primero quitamos únicamente cualquier tinte negro puesto por
             * USU. Después reconstruimos la barra completa de survivors. Esto
             * es importante: CharacterSelectController.RebuildLocal por sí
             * solo no invalida todos los datos que CharacterSelectBarController
             * mantiene para tooltip/disponibilidad.
             */
            ReleaseAllPortraitOverrides();

            int rebuiltBars =
                RebuildCharacterSelectBars();

            int rebuiltSelectors =
                RebuildCharacterSelectControllers();

            /*
             * La ruta prioritaria ya no es FindObjectsOfType. ScrollableLobbyUI
             * mantiene los SurvivorIconController dentro del allocator de la
             * propia CharacterSelectBarController; esa es la fuente real de
             * las celdas que el usuario está viendo.
             */
            int refreshedIcons =
                RefreshSurvivorIconsFromBars();

            if (refreshedIcons == 0)
            {
                refreshedIcons =
                    RefreshSurvivorIcons();
            }

            Canvas.ForceUpdateCanvases();

            if (!string.IsNullOrWhiteSpace(normalizedBodyName))
            {
                bool selectorUiPresent =
                    rebuiltBars > 0 ||
                    rebuiltSelectors > 0 ||
                    refreshedIcons > 0;


                if (selectorUiPresent)
                {
                    ScheduleDeferredUiRefresh(
                        normalizedBodyName
                    );

                    UsuLog.Verbose(
                        Logger,
                        $"[MISSION UI] Selector refrescado | " +
                        $"Body: {normalizedBodyName} | " +
                        $"Bars: {rebuiltBars} | " +
                        $"Selectors: {rebuiltSelectors} | " +
                        $"Icons: {refreshedIcons} | " +
                        $"Deferred: armado"
                    );
                }
                else
                {
                    // En una Run normal no existe selector de survivors.
                    // No armamos un retry inútil ni emitimos warnings falsos.
                    PendingUiRefreshTicks = 0;
                    PendingUiRefreshBody = "";
                }
            }
        }

        private static void SurvivorIconController_Rebuild(
            On.RoR2.UI.SurvivorIconController.orig_Rebuild orig,
            SurvivorIconController self
        )
        {
            PrepareForVanillaRefresh(
                self
            );

            CaptureBaseColorIfNeeded(
                self
            );

            orig(self);

            RefreshPortrait(
                self
            );
        }

        private static void SurvivorIconController_UpdateAvailability(
            On.RoR2.UI.SurvivorIconController.orig_UpdateAvailability orig,
            SurvivorIconController self
        )
        {
            PrepareForVanillaRefresh(
                self
            );

            CaptureBaseColorIfNeeded(
                self
            );

            orig(self);

            RefreshPortrait(
                self
            );
        }

        /// <summary>
        /// Si el icono dejó de estar controlado por USU, elimina nuestro
        /// override ANTES de que RoR2 recalcule disponibilidad. De esta forma
        /// no restauramos un color antiguo encima del estado Original.
        /// </summary>
        private static void PrepareForVanillaRefresh(
            SurvivorIconController self
        )
        {
            if (
                self == null ||
                self.survivorDef == null ||
                self.survivorIcon == null
            )
            {
                return;
            }

            /*
             * Nunca dejamos nuestro negro como entrada para el cálculo de
             * RoR2. Primero restauramos la base que habíamos guardado y luego
             * dejamos que Rebuild/UpdateAvailability recalculen el estado.
             */
            RestoreOurOverrideOnly(
                self
            );
        }

        private static void CaptureBaseColorIfNeeded(
            SurvivorIconController self
        )
        {
            if (
                self == null ||
                self.survivorDef == null ||
                self.survivorIcon == null ||
                !ShouldManageVisual(self)
            )
            {
                return;
            }

            if (
                !OriginalColors.ContainsKey(
                    self
                )
            )
            {
                OriginalColors[self] =
                    self.survivorIcon.color;
            }
        }

        private static bool ShouldManageVisual(
            SurvivorIconController self
        )
        {
            if (
                self == null ||
                self.survivorDef == null
            )
            {
                return false;
            }

            return
                SurvivorUnlockManager.IsCustomUnlock(
                    self.survivorDef.unlockableDef
                ) ||
                IsRuntimeManagedBody(
                    self.survivorDef
                );
        }

        private static void RefreshPortrait(
            SurvivorIconController self
        )
        {
            if (
                self == null ||
                self.survivorDef == null ||
                self.survivorIcon == null
            )
            {
                return;
            }

            UnlockableDef unlockable =
                self.survivorDef.unlockableDef;

            bool activeIsUsu =
                SurvivorUnlockManager.IsCustomUnlock(
                    unlockable
                );

            bool runtimeManaged =
                IsRuntimeManagedBody(
                    self.survivorDef
                );

            /*
             * Para survivors que nunca cambiaron de provider en esta sesión
             * y que están usando Original, USU sigue sin tocar el visual.
             *
             * Para un survivor que SÍ fue cambiado en runtime, la
             * disponibilidad continúa siendo la de RoR2
             * (survivorIsUnlocked), pero USU asegura que un false no quede a
             * color por un cache viejo del selector.
             */
            if (
                !activeIsUsu &&
                !runtimeManaged
            )
            {
                return;
            }

            /*
             * Si el UnlockableDef activo está realmente desbloqueado dejamos
             * intacto el resultado visual de RoR2.
             */
            if (self.survivorIsUnlocked)
            {
                RestoreOurOverrideOnly(
                    self
                );

                return;
            }

            ApplyLockedColor(
                self
            );
        }

        /// <summary>
        /// Reutiliza en el navegador USU el mismo lenguaje visual aplicado
        /// por este manager al Character Select: el portrait real permanece
        /// como fuente y, mientras la identidad está bloqueada, se convierte
        /// en silueta negra en vez de sustituirse por un icono genérico.
        /// </summary>
        internal static void ApplyBrowserPortrait(
            RawImage portrait,
            SurvivorContentProfile profile
        )
        {
            if (portrait == null || profile == null)
            {
                return;
            }

            portrait.texture = profile.Portrait;

            if (profile.Portrait == null)
            {
                portrait.color =
                    new Color(0.06f, 0.09f, 0.12f, 1f);

                return;
            }

            portrait.color =
                profile.ShouldHideIdentity
                    ? new Color(0f, 0f, 0f, 1f)
                    : Color.white;
        }


        private static void ApplyLockedColor(
            SurvivorIconController self
        )
        {
            if (
                !OriginalColors.ContainsKey(
                    self
                )
            )
            {
                /*
                 * Fallback defensivo para una llamada que no haya pasado por
                 * Rebuild/UpdateAvailability. En el flujo normal la base se
                 * captura antes de ejecutar el cálculo vanilla.
                 */
                OriginalColors[self] =
                    self.survivorIcon.color;
            }

            UsuLog.Verbose(
                Logger,
                $"Silueta bloqueada aplicada: " +
                $"{self.survivorDef.cachedName}"
            );

            Color original =
                OriginalColors[self];

            self.survivorIcon.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    original.a
                );
        }

        /// <summary>
        /// Restaura sólo el color que USU había sustituido y elimina su cache.
        /// No intenta decidir si Original debe verse bloqueado o desbloqueado.
        /// Esa decisión se deja al Rebuild/UpdateAvailability de RoR2.
        /// </summary>
        private static void RestoreOurOverrideOnly(
            SurvivorIconController self
        )
        {
            if (
                self == null ||
                self.survivorIcon == null
            )
            {
                return;
            }

            if (
                !OriginalColors.TryGetValue(
                    self,
                    out Color original
                )
            )
            {
                return;
            }

            self.survivorIcon.color =
                original;

            OriginalColors.Remove(
                self
            );

            UsuLog.Verbose(
                Logger,
                $"Override visual USU liberado: " +
                $"{self.survivorDef?.cachedName ?? "<null>"}"
            );
        }

        private static void ReleaseAllPortraitOverrides()
        {
            if (OriginalColors.Count == 0)
            {
                return;
            }

            KeyValuePair<
                SurvivorIconController,
                Color
            >[] snapshot =
                new KeyValuePair<
                    SurvivorIconController,
                    Color
                >[OriginalColors.Count];

            int index = 0;

            foreach (
                KeyValuePair<
                    SurvivorIconController,
                    Color
                > pair
                in OriginalColors
            )
            {
                snapshot[index++] =
                    pair;
            }

            OriginalColors.Clear();

            foreach (
                KeyValuePair<
                    SurvivorIconController,
                    Color
                > pair
                in snapshot
            )
            {
                SurvivorIconController controller =
                    pair.Key;

                if (
                    controller == null ||
                    controller.survivorIcon == null
                )
                {
                    continue;
                }

                controller.survivorIcon.color =
                    pair.Value;
            }
        }

        private static int RebuildCharacterSelectBars()
        {
            CharacterSelectBarController[] bars =
                UnityEngine.Object.FindObjectsOfType<
                    CharacterSelectBarController
                >();

            if (
                bars == null ||
                bars.Length == 0
            )
            {
                return 0;
            }

            MethodInfo build =
                FindParameterlessInstanceMethod(
                    typeof(CharacterSelectBarController),
                    "Build"
                );

            MethodInfo enforceValidChoice =
                FindParameterlessInstanceMethod(
                    typeof(CharacterSelectBarController),
                    "EnforceValidChoice"
                );

            if (build == null)
            {
                Logger?.LogWarning(
                    "[MISSION UI] No se encontró " +
                    "CharacterSelectBarController.Build."
                );

                return 0;
            }

            int rebuilt =
                0;

            foreach (
                CharacterSelectBarController bar
                in bars
            )
            {
                if (
                    bar == null ||
                    bar.gameObject == null ||
                    !bar.gameObject.activeInHierarchy
                )
                {
                    continue;
                }

                try
                {
                    build.Invoke(
                        bar,
                        null
                    );

                    enforceValidChoice?.Invoke(
                        bar,
                        null
                    );

                    rebuilt++;
                }
                catch (Exception ex)
                {
                    Logger?.LogWarning(
                        "[MISSION UI] No se pudo reconstruir " +
                        "CharacterSelectBarController: " +
                        ex.GetType().Name
                    );
                }
            }

            return rebuilt;
        }

        private static int RebuildCharacterSelectControllers()
        {
            CharacterSelectController[] controllers =
                UnityEngine.Object.FindObjectsOfType<
                    CharacterSelectController
                >();

            if (
                controllers == null ||
                controllers.Length == 0
            )
            {
                return 0;
            }

            /*
             * En la versión actual de RoR2 la firma real es:
             *
             *     RebuildLocal(bool)
             *
             * FIX4 buscaba por error una sobrecarga sin parámetros. Por eso
             * el log mostraba Bars: 1 pero Selectors: 0 y el tooltip seguía
             * conservando el UnlockableDef anterior aunque el provider real
             * ya hubiera cambiado.
             *
             * Priorizamos la firma actual con bool y mantenemos el fallback
             * sin parámetros por compatibilidad con otras versiones.
             */
            MethodInfo rebuildLocalWithBool =
                typeof(CharacterSelectController).GetMethod(
                    "RebuildLocal",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic,
                    null,
                    new Type[]
                    {
                        typeof(bool)
                    },
                    null
                );

            MethodInfo rebuildLocalWithoutArgs =
                FindParameterlessInstanceMethod(
                    typeof(CharacterSelectController),
                    "RebuildLocal"
                );

            if (
                rebuildLocalWithBool == null &&
                rebuildLocalWithoutArgs == null
            )
            {
                Logger?.LogWarning(
                    "[MISSION UI] No se encontró una firma compatible de " +
                    "CharacterSelectController.RebuildLocal."
                );

                return 0;
            }

            int rebuilt =
                0;

            foreach (
                CharacterSelectController controller
                in controllers
            )
            {
                if (
                    controller == null ||
                    controller.gameObject == null ||
                    !controller.gameObject.activeInHierarchy
                )
                {
                    continue;
                }

                try
                {
                    if (rebuildLocalWithBool != null)
                    {
                        /*
                         * false coincide con el uso normal del lobby: fuerza
                         * a reconstruir el selector local sin tratarlo como una
                         * reconstrucción especial de selección inicial.
                         */
                        rebuildLocalWithBool.Invoke(
                            controller,
                            new object[]
                            {
                                false
                            }
                        );
                    }
                    else
                    {
                        rebuildLocalWithoutArgs.Invoke(
                            controller,
                            null
                        );
                    }

                    rebuilt++;
                }
                catch (Exception ex)
                {
                    Logger?.LogWarning(
                        "[MISSION UI] No se pudo ejecutar " +
                        "CharacterSelectController.RebuildLocal: " +
                        ex.GetType().Name
                    );
                }
            }

            return rebuilt;
        }

        private static void ScheduleDeferredUiRefresh(
            string bodyName
        )
        {
            PendingUiRefreshBody =
                bodyName?.Trim() ?? "";

            PendingUiRefreshTicks =
                MaxPendingUiRefreshTicks;
        }

        private static void OnFixedUpdate()
        {
            if (PendingUiRefreshTicks <= 0)
            {
                return;
            }

            PendingUiRefreshTicks--;

            int refreshedIcons =
                RefreshSurvivorIconsFromBars();

            if (refreshedIcons == 0)
            {
                refreshedIcons =
                    RefreshSurvivorIcons();
            }

            if (refreshedIcons > 0)
            {
                Canvas.ForceUpdateCanvases();

                UsuLog.Verbose(
                    Logger,
                    $"[MISSION UI] Refresh diferido aplicado | " +
                    $"Body: {PendingUiRefreshBody} | " +
                    $"Icons: {refreshedIcons}"
                );

                PendingUiRefreshTicks = 0;
                PendingUiRefreshBody = "";

                return;
            }

            if (PendingUiRefreshTicks == 0)
            {
                UsuLog.Verbose(
                    Logger,
                    $"[MISSION UI] Refresh diferido agotado sin iconos | " +
                    $"Body: {PendingUiRefreshBody}"
                );

                PendingUiRefreshBody = "";
            }
        }

        /// <summary>
        /// Refresca directamente las celdas administradas por
        /// CharacterSelectBarController.survivorIconControllers.
        ///
        /// Esto evita depender de FindObjectsOfType justo en el frame donde
        /// ScrollableLobbyUI está reconstruyendo/reutilizando sus celdas.
        /// </summary>
        private static int RefreshSurvivorIconsFromBars()
        {
            CharacterSelectBarController[] bars =
                UnityEngine.Object.FindObjectsOfType<
                    CharacterSelectBarController
                >();

            if (
                bars == null ||
                bars.Length == 0
            )
            {
                return 0;
            }

            FieldInfo allocatorField =
                typeof(CharacterSelectBarController).GetField(
                    "survivorIconControllers",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

            if (allocatorField == null)
            {
                UsuLog.Verbose(
                    Logger,
                    "Refresh UI: no se encontró survivorIconControllers."
                );

                return 0;
            }

            MethodInfo rebuild =
                FindParameterlessInstanceMethod(
                    typeof(SurvivorIconController),
                    "Rebuild"
                );

            MethodInfo updateAvailability =
                FindParameterlessInstanceMethod(
                    typeof(SurvivorIconController),
                    "UpdateAvailability"
                );

            int refreshed =
                0;

            HashSet<SurvivorIconController> seen =
                new HashSet<SurvivorIconController>();

            foreach (
                CharacterSelectBarController bar
                in bars
            )
            {
                if (
                    bar == null ||
                    bar.gameObject == null ||
                    !bar.gameObject.activeInHierarchy
                )
                {
                    continue;
                }

                object allocator;

                try
                {
                    allocator =
                        allocatorField.GetValue(
                            bar
                        );
                }
                catch
                {
                    continue;
                }

                if (allocator == null)
                {
                    continue;
                }

                FieldInfo elementsField =
                    allocator.GetType().GetField(
                        "elements",
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic
                    );

                PropertyInfo elementsProperty =
                    allocator.GetType().GetProperty(
                        "elements",
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic
                    );

                object elementsObject = null;

                try
                {
                    if (elementsField != null)
                    {
                        elementsObject =
                            elementsField.GetValue(
                                allocator
                            );
                    }
                    else if (elementsProperty != null)
                    {
                        elementsObject =
                            elementsProperty.GetValue(
                                allocator,
                                null
                            );
                    }
                }
                catch
                {
                    elementsObject = null;
                }

                if (!(elementsObject is IEnumerable elements))
                {
                    continue;
                }

                foreach (object item in elements)
                {
                    SurvivorIconController controller =
                        item as SurvivorIconController;

                    if (
                        controller == null ||
                        controller.gameObject == null ||
                        !controller.gameObject.activeInHierarchy ||
                        !seen.Add(controller)
                    )
                    {
                        continue;
                    }

                    try
                    {
                        /*
                         * Rebuild es el paso importante para los tokens del
                         * tooltip. UpdateAvailability recalcula además si el
                         * usuario puede seleccionarlo con el UnlockableDef
                         * que está activo ahora.
                         */
                        rebuild?.Invoke(
                            controller,
                            null
                        );

                        updateAvailability?.Invoke(
                            controller,
                            null
                        );

                        refreshed++;
                    }
                    catch (Exception ex)
                    {
                        UsuLog.Verbose(
                            Logger,
                            $"Refresh UI allocator: icono falló para " +
                            $"{controller.survivorDef?.cachedName ?? "<null>"}: " +
                            ex.GetType().Name
                        );
                    }
                }
            }

            return refreshed;
        }

        private static int RefreshSurvivorIcons()
        {
            SurvivorIconController[] controllers =
                UnityEngine.Object.FindObjectsOfType<
                    SurvivorIconController
                >();

            if (
                controllers == null ||
                controllers.Length == 0
            )
            {
                return 0;
            }

            MethodInfo rebuild =
                FindParameterlessInstanceMethod(
                    typeof(SurvivorIconController),
                    "Rebuild"
                );

            MethodInfo updateAvailability =
                FindParameterlessInstanceMethod(
                    typeof(SurvivorIconController),
                    "UpdateAvailability"
                );

            int refreshed =
                0;

            foreach (
                SurvivorIconController controller
                in controllers
            )
            {
                if (
                    controller == null ||
                    controller.gameObject == null ||
                    !controller.gameObject.activeInHierarchy
                )
                {
                    continue;
                }

                try
                {
                    rebuild?.Invoke(
                        controller,
                        null
                    );

                    updateAvailability?.Invoke(
                        controller,
                        null
                    );

                    refreshed++;
                }
                catch (Exception ex)
                {
                    UsuLog.Verbose(
                        Logger,
                        $"Refresh UI: icono falló para " +
                        $"{controller.survivorDef?.cachedName ?? "<null>"}: " +
                        ex.GetType().Name
                    );
                }
            }

            return refreshed;
        }

        private static bool IsRuntimeManagedBody(
            SurvivorDef survivorDef
        )
        {
            if (
                survivorDef == null ||
                survivorDef.bodyPrefab == null
            )
            {
                return false;
            }

            return RuntimeManagedBodies.Contains(
                survivorDef.bodyPrefab.name
            );
        }

        private static MethodInfo FindParameterlessInstanceMethod(
            Type type,
            string methodName
        )
        {
            if (
                type == null ||
                string.IsNullOrWhiteSpace(methodName)
            )
            {
                return null;
            }

            return type.GetMethod(
                methodName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null
            );
        }
    }
}
