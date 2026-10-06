using System;
using System.Collections.Generic;
using UnityEngine;
#if UNICORTEX_UGUI
using UniCortex.Editor.Infrastructures;
#endif

namespace UniCortex.Editor.Testing
{
    /// <summary>
    /// Finds the UI that can be pressed now, with the same rules as the <c>get_ui_pointer_targets</c> tool.
    /// For tests in Play Mode, e.g. a monkey test that keeps pressing random UI. Requires the uGUI package
    /// (com.unity.ugui).
    /// </summary>
    /// <remarks>
    /// Call these from the player loop, e.g. from a <c>[UnityTest]</c> coroutine or <c>MonoBehaviour.Update</c>.
    /// <c>Screen.width</c> / <c>Screen.height</c>, which the EventSystem raycast uses, return the Game View
    /// resolution only there (from <c>EditorApplication.update</c> they return the size of another view).
    /// </remarks>
    public static class UiPointerTargets
    {
        /// <summary>
        /// Returns the uGUI objects that can be pressed now, in Hierarchy order across every loaded scene.
        /// An object is returned when it is active and under a Canvas, has an enabled component handling pointer
        /// events, is interactable, and is the topmost EventSystem raycast hit at its center (or a parent of it).
        /// </summary>
        /// <exception cref="InvalidOperationException">There is no active EventSystem.</exception>
        public static IReadOnlyList<UiPointerTarget> Find()
        {
#if UNICORTEX_UGUI
            return UguiPointerTargetFinder.Find();
#else
            throw CreateNotSupportedException();
#endif
        }

        /// <summary>
        /// Returns the center of a uGUI object in Game View coordinates (origin at the bottom-left), the position
        /// the <c>click_mouse</c> tool uses for an instanceId.
        /// </summary>
        /// <exception cref="ArgumentException">The object is not a RectTransform under a Canvas.</exception>
        public static Vector2 GetCenter(GameObject target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

#if UNICORTEX_UGUI
            return UguiPointerTargetFinder.GetCenter(target);
#else
            throw CreateNotSupportedException();
#endif
        }

#if !UNICORTEX_UGUI
        private static NotSupportedException CreateNotSupportedException()
        {
            return new NotSupportedException(
                "uGUI package (com.unity.ugui) is not installed. " +
                "Install it via Unity Package Manager to use this feature.");
        }
#endif
    }
}
