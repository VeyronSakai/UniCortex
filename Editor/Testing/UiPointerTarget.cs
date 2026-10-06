using UnityEngine;

namespace UniCortex.Editor.Testing
{
    /// <summary>
    /// A UI object that can be pressed now, found by <see cref="UiPointerTargets.Find"/>.
    /// </summary>
    public sealed class UiPointerTarget
    {
        public GameObject GameObject { get; }

        /// <summary>
        /// Hierarchy path of the object, e.g. "Canvas/Menu/StartButton".
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Rect of the object in Game View coordinates (origin at the bottom-left), the same coordinates as
        /// <c>Mouse.current.position</c> and <see cref="PointerInput.Click(Vector2)"/>.
        /// </summary>
        public Rect ScreenRect { get; }

        internal UiPointerTarget(GameObject gameObject, string path, Rect screenRect)
        {
            GameObject = gameObject;
            Path = path;
            ScreenRect = screenRect;
        }
    }
}
