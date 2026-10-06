#if UNICORTEX_UGUI
using System;
using System.Collections.Generic;
using System.Linq;
using UniCortex.Editor.Testing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace UniCortex.Editor.Infrastructures
{
    // Finds uGUI objects that can be pressed now. Shared by get_ui_pointer_targets / the mouse tools and the public
    // API for tests (UiPointerTargets), so that they agree on what "can be pressed" means.
    // Must be called inside the player loop: GraphicRaycaster uses Screen.width / Screen.height, which return the
    // Game View resolution only while the player loop runs.
    internal static class UguiPointerTargetFinder
    {
        private static readonly Type[] s_pointerHandlerTypes =
        {
            typeof(IPointerEnterHandler),
            typeof(IPointerExitHandler),
            typeof(IPointerDownHandler),
            typeof(IPointerUpHandler),
            typeof(IPointerClickHandler),
            typeof(IInitializePotentialDragHandler),
            typeof(IBeginDragHandler),
            typeof(IDragHandler),
            typeof(IEndDragHandler),
            typeof(IDropHandler),
            typeof(IScrollHandler),
        };

        // EventTrigger implements every handler interface, so its registered entries are checked instead.
        private static readonly HashSet<EventTriggerType> s_pointerEventTriggerTypes = new()
        {
            EventTriggerType.PointerEnter,
            EventTriggerType.PointerExit,
            EventTriggerType.PointerDown,
            EventTriggerType.PointerUp,
            EventTriggerType.PointerClick,
            EventTriggerType.InitializePotentialDrag,
            EventTriggerType.BeginDrag,
            EventTriggerType.Drag,
            EventTriggerType.EndDrag,
            EventTriggerType.Drop,
            EventTriggerType.Scroll,
        };

        public static List<UiPointerTarget> Find()
        {
            var eventSystem = GetEventSystem();

            var targets = new List<UiPointerTarget>();
            foreach (var scene in LoadedScenes.Get())
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    CollectTargets(root.transform, eventSystem, targets);
                }
            }

            return targets;
        }

        // Returns the center of a uGUI object in Game View coordinates.
        public static Vector2 GetCenter(GameObject gameObject)
        {
            if (!(gameObject.transform is RectTransform rectTransform) || GetCanvas(rectTransform) == null)
            {
                throw new ArgumentException(
                    $"'{GetPath(gameObject.transform)}' is not a uGUI element (a RectTransform under a Canvas). " +
                    "Only uGUI elements are supported for now.");
            }

            return GetScreenCenter(rectTransform);
        }

        // Collects objects that can be pressed now: active, interactable, handling pointer events,
        // and hit first by the EventSystem raycast at their center.
        private static void CollectTargets(Transform transform, EventSystem eventSystem,
            List<UiPointerTarget> targets)
        {
            if (!transform.gameObject.activeInHierarchy)
            {
                return;
            }

            if (transform is RectTransform rectTransform && GetCanvas(rectTransform) != null
                                                          && HasPointerEventHandler(transform.gameObject)
                                                          && IsInteractable(transform.gameObject))
            {
                var center = GetScreenCenter(rectTransform);
                if (ReceivesPointer(transform.gameObject, center, eventSystem))
                {
                    targets.Add(new UiPointerTarget(transform.gameObject, GetPath(transform),
                        GetScreenRect(rectTransform)));
                }
            }

            foreach (Transform child in transform)
            {
                CollectTargets(child, eventSystem, targets);
            }
        }

        private static bool HasPointerEventHandler(GameObject gameObject)
        {
            foreach (var behaviour in gameObject.GetComponents<MonoBehaviour>())
            {
                // Missing scripts are null. Disabled components do not receive events.
                if (behaviour == null || !behaviour.enabled)
                {
                    continue;
                }

                if (behaviour is EventTrigger eventTrigger)
                {
                    if (eventTrigger.triggers.Any(entry => s_pointerEventTriggerTypes.Contains(entry.eventID)))
                    {
                        return true;
                    }

                    continue;
                }

                if (s_pointerHandlerTypes.Any(type => type.IsInstanceOfType(behaviour)))
                {
                    return true;
                }
            }

            return false;
        }

        // Selectable.IsInteractable() also takes CanvasGroup.interactable into account.
        private static bool IsInteractable(GameObject gameObject)
        {
            var selectable = gameObject.GetComponent<Selectable>();
            return selectable == null || selectable.IsInteractable();
        }

        private static Vector2 GetScreenCenter(RectTransform rectTransform)
        {
            var camera = GetEventCamera(GetCanvas(rectTransform));
            return RectTransformUtility.WorldToScreenPoint(camera,
                rectTransform.TransformPoint(rectTransform.rect.center));
        }

        private static Rect GetScreenRect(RectTransform rectTransform)
        {
            var camera = GetEventCamera(GetCanvas(rectTransform));

            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var corner in corners)
            {
                var point = RectTransformUtility.WorldToScreenPoint(camera, corner);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        // True when a pointer event at the position reaches the target, that is, when the topmost EventSystem
        // raycast hit there is the target or its child (the EventSystem looks for a handler from the hit object
        // up through its parents).
        private static bool ReceivesPointer(GameObject target, Vector2 position, EventSystem eventSystem)
        {
            using var _ = ListPool<RaycastResult>.Get(out var raycastResults);
            eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = position }, raycastResults);
            return raycastResults.Count > 0 && raycastResults[0].gameObject.transform.IsChildOf(target.transform);
        }

        private static Canvas GetCanvas(RectTransform rectTransform)
        {
            var canvas = rectTransform.GetComponentInParent<Canvas>(true);
            return canvas != null ? canvas.rootCanvas : null;
        }

        // Same camera the Canvas's raycaster uses to convert between world and screen space.
        private static Camera GetEventCamera(Canvas canvas)
        {
            var raycaster = canvas.GetComponent<BaseRaycaster>();
            if (raycaster != null)
            {
                return raycaster.eventCamera;
            }

            return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }

        private static string GetPath(Transform transform)
        {
            var names = new List<string>();
            for (var current = transform; current != null; current = current.parent)
            {
                names.Add(current.name);
            }

            names.Reverse();
            return string.Join("/", names);
        }

        private static EventSystem GetEventSystem()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                throw new InvalidOperationException(
                    "No active EventSystem in the scene. UI cannot receive pointer events without one.");
            }

            return eventSystem;
        }
    }
}
#endif
