using System.Collections;
using UniCortex.Editor.Infrastructures;
using UnityEngine;
using MouseButton = UniCortex.Editor.Domains.Models.MouseButton;

namespace UniCortex.Editor.Testing
{
    /// <summary>
    /// Presses UI through the Input System and the EventSystem raycast like a real tap, in the same way as the
    /// <c>click_mouse</c> tool. A target covered by other UI does not receive the event. For tests in Play Mode.
    /// Requires the Input System package (com.unity.inputsystem).
    /// </summary>
    /// <remarks>
    /// The returned enumerators run over several frames: <c>yield return</c> them from a <c>[UnityTest]</c>
    /// coroutine, or wrap them for async code (e.g. <c>UniTask.ToUniTask</c>).
    /// Input is sent to the game even when the Game View is not focused, by changing the Input System settings
    /// in the same way as the mouse tools. Unity restores them when Play Mode exits.
    /// </remarks>
    public static class PointerInput
    {
        /// <summary>
        /// Clicks (taps) the center of a uGUI object (see <see cref="UiPointerTargets.GetCenter"/>) with the left
        /// button. Completes after the game has processed the release (e.g. <c>Button.onClick</c> has been
        /// invoked).
        /// </summary>
        public static IEnumerator Click(GameObject target)
        {
            return Click(UiPointerTargets.GetCenter(target));
        }

        /// <summary>
        /// Clicks (taps) the position in Game View coordinates (origin at the bottom-left) with the left button.
        /// Completes after the game has processed the release (e.g. <c>Button.onClick</c> has been invoked).
        /// </summary>
        public static IEnumerator Click(Vector2 position)
        {
            var operations = SharedInputOperations.Instance;

            operations.PressMouseButton(position.x, position.y, MouseButton.Left);
            yield return null;

            operations.ReleaseMouseButton(position.x, position.y, MouseButton.Left);

            // Input queued in a frame is processed in the next frame, before MonoBehaviour.Update and the
            // EventSystem, so wait one more frame until the game has reacted to the release.
            yield return null;
        }
    }
}
