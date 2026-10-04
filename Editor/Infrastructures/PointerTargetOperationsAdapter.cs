#if UNICORTEX_UGUI
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class PointerTargetOperationsAdapter : IPointerTargetOperations
    {
        // Pointer event interfaces in the order they are reported.
        private static readonly (Type type, string name)[] s_pointerHandlers =
        {
            (typeof(IPointerEnterHandler), PointerEventName.Enter),
            (typeof(IPointerExitHandler), PointerEventName.Exit),
            (typeof(IPointerDownHandler), PointerEventName.Down),
            (typeof(IPointerUpHandler), PointerEventName.Up),
            (typeof(IPointerClickHandler), PointerEventName.Click),
            (typeof(IInitializePotentialDragHandler), PointerEventName.InitializePotentialDrag),
            (typeof(IBeginDragHandler), PointerEventName.BeginDrag),
            (typeof(IDragHandler), PointerEventName.Drag),
            (typeof(IEndDragHandler), PointerEventName.EndDrag),
            (typeof(IDropHandler), PointerEventName.Drop),
            (typeof(IScrollHandler), PointerEventName.Scroll),
        };

        // EventTrigger implements every handler interface, so its registered entries are used instead.
        private static readonly Dictionary<EventTriggerType, string> s_eventTriggerTypes = new()
        {
            { EventTriggerType.PointerEnter, PointerEventName.Enter },
            { EventTriggerType.PointerExit, PointerEventName.Exit },
            { EventTriggerType.PointerDown, PointerEventName.Down },
            { EventTriggerType.PointerUp, PointerEventName.Up },
            { EventTriggerType.PointerClick, PointerEventName.Click },
            { EventTriggerType.InitializePotentialDrag, PointerEventName.InitializePotentialDrag },
            { EventTriggerType.BeginDrag, PointerEventName.BeginDrag },
            { EventTriggerType.Drag, PointerEventName.Drag },
            { EventTriggerType.EndDrag, PointerEventName.EndDrag },
            { EventTriggerType.Drop, PointerEventName.Drop },
            { EventTriggerType.Scroll, PointerEventName.Scroll },
        };

        public Task<List<PointerTarget>> GetPointerTargetsAsync(CancellationToken cancellationToken)
        {
            return PlayerLoopRunner.RunAsync(GetPointerTargets, cancellationToken);
        }

        public Task<PointerTarget> GetPointerTargetAsync(int instanceId, string path,
            CancellationToken cancellationToken)
        {
            return PlayerLoopRunner.RunAsync(() => GetPointerTarget(instanceId, path), cancellationToken);
        }

        private static List<PointerTarget> GetPointerTargets()
        {
            var eventSystem = GetEventSystem();

            var targets = new List<PointerTarget>();
            var raycastResults = new List<RaycastResult>();
            foreach (var scene in LoadedScenes.Get())
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    CollectTargets(root.transform, eventSystem, raycastResults, targets);
                }
            }

            return targets;
        }

        private static PointerTarget GetPointerTarget(int instanceId, string path)
        {
            var eventSystem = GetEventSystem();

            var gameObject = instanceId != 0 ? FindByInstanceId(instanceId) : FindByPath(path);
            if (!(gameObject.transform is RectTransform rectTransform) || GetCanvas(rectTransform) == null)
            {
                throw new ArgumentException(
                    $"'{GetPath(gameObject.transform)}' is not a UI element (a RectTransform under a Canvas).");
            }

            return BuildTarget(rectTransform, GetPointerEvents(gameObject), eventSystem,
                new List<RaycastResult>());
        }

        private static void CollectTargets(Transform transform, EventSystem eventSystem,
            List<RaycastResult> raycastResults, List<PointerTarget> targets)
        {
            if (transform is RectTransform rectTransform && GetCanvas(rectTransform) != null)
            {
                var events = GetPointerEvents(transform.gameObject);
                if (events.Count > 0)
                {
                    targets.Add(BuildTarget(rectTransform, events, eventSystem, raycastResults));
                }
            }

            foreach (Transform child in transform)
            {
                CollectTargets(child, eventSystem, raycastResults, targets);
            }
        }

        private static List<string> GetPointerEvents(GameObject gameObject)
        {
            var events = new HashSet<string>();
            foreach (var behaviour in gameObject.GetComponents<MonoBehaviour>())
            {
                // Missing scripts are null. Disabled components do not receive events.
                if (behaviour == null || !behaviour.enabled)
                {
                    continue;
                }

                if (behaviour is EventTrigger eventTrigger)
                {
                    foreach (var entry in eventTrigger.triggers)
                    {
                        if (s_eventTriggerTypes.TryGetValue(entry.eventID, out var name))
                        {
                            events.Add(name);
                        }
                    }

                    continue;
                }

                foreach (var (type, name) in s_pointerHandlers)
                {
                    if (type.IsInstanceOfType(behaviour))
                    {
                        events.Add(name);
                    }
                }
            }

            return s_pointerHandlers.Select(h => h.name).Where(events.Contains).ToList();
        }

        private static PointerTarget BuildTarget(RectTransform rectTransform, List<string> events,
            EventSystem eventSystem, List<RaycastResult> raycastResults)
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

            var center = RectTransformUtility.WorldToScreenPoint(camera,
                rectTransform.TransformPoint(rectTransform.rect.center));

            var gameObject = rectTransform.gameObject;
            var blockedBy = Raycast(gameObject, center, eventSystem, raycastResults, out var blocked);
            var selectable = gameObject.GetComponent<Selectable>();

            return new PointerTarget(
                gameObject.name,
                GetPath(rectTransform),
                gameObject.GetInstanceID(),
                center.x,
                center.y,
                new ScreenRect(min.x, min.y, max.x - min.x, max.y - min.y),
                events,
                gameObject.activeInHierarchy,
                selectable == null || selectable.IsInteractable(),
                blocked,
                blockedBy);
        }

        // Returns the Hierarchy path of the topmost hit when it is neither the target nor its child.
        private static string Raycast(GameObject target, Vector2 position, EventSystem eventSystem,
            List<RaycastResult> raycastResults, out bool blocked)
        {
            raycastResults.Clear();
            eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = position }, raycastResults);

            if (raycastResults.Count == 0)
            {
                blocked = true;
                return "";
            }

            var hit = raycastResults[0].gameObject.transform;
            blocked = !hit.IsChildOf(target.transform);
            return blocked ? GetPath(hit) : "";
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

        private static GameObject FindByInstanceId(int instanceId)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            var gameObject = obj as GameObject;
            if (gameObject == null && obj is Component component)
            {
                gameObject = component.gameObject;
            }

            if (gameObject == null)
            {
                throw new ArgumentException($"GameObject with instanceId {instanceId} not found.");
            }

            return gameObject;
        }

        private static GameObject FindByPath(string path)
        {
            var names = path.Trim('/').Split('/');
            var matches = new List<Transform>();
            foreach (var scene in LoadedScenes.Get())
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.name == names[0])
                    {
                        CollectPathMatches(root.transform, names, 1, matches);
                    }
                }
            }

            if (matches.Count == 0)
            {
                throw new ArgumentException($"GameObject at path '{path}' not found.");
            }

            if (matches.Count > 1)
            {
                var ids = string.Join(", ", matches.Select(m => m.gameObject.GetInstanceID()));
                throw new ArgumentException(
                    $"Multiple GameObjects match path '{path}' (instanceIds: {ids}). Specify targetInstanceId instead.");
            }

            return matches[0].gameObject;
        }

        private static void CollectPathMatches(Transform transform, string[] names, int depth,
            List<Transform> matches)
        {
            if (depth == names.Length)
            {
                matches.Add(transform);
                return;
            }

            foreach (Transform child in transform)
            {
                if (child.name == names[depth])
                {
                    CollectPathMatches(child, names, depth + 1, matches);
                }
            }
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
