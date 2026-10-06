#if UNICORTEX_UGUI && UNICORTEX_INPUT_SYSTEM
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UniCortex.Editor.Testing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace UniCortex.Tests
{
    // The public API for tests (UiPointerTargets / PointerInput) works only inside the player loop in Play Mode,
    // so these are Play Mode tests.
    [TestFixture]
    internal sealed class PointerApiTest
    {
        private static readonly Vector2 s_buttonSize = new(100f, 100f);

        private GameObject _eventSystem;
        private GameObject _canvas;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _eventSystem = CreateEventSystem();

            _canvas = new GameObject("PointerApiTest_Canvas", typeof(Canvas), typeof(GraphicRaycaster));
            _canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            // Wait a frame so that the Canvas is sized to the screen.
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_canvas);
            Object.DestroyImmediate(_eventSystem);
        }

        [UnityTest]
        public IEnumerator Find_ReturnsInteractableButton()
        {
            // Arrange
            var button = CreateButton("Button", Vector2.zero);
            yield return WaitForCanvasRebuild();

            // Act
            var targets = UiPointerTargets.Find();

            // Assert
            var target = targets.Single(t => t.GameObject == button.gameObject);
            Assert.AreEqual("PointerApiTest_Canvas/Button", target.Path);
            Assert.AreEqual(s_buttonSize.x, target.ScreenRect.width, 0.01f);
            Assert.AreEqual(s_buttonSize.y, target.ScreenRect.height, 0.01f);
            Assert.AreEqual(UiPointerTargets.GetCenter(button.gameObject).x, target.ScreenRect.center.x, 0.01f);
            Assert.AreEqual(UiPointerTargets.GetCenter(button.gameObject).y, target.ScreenRect.center.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator Find_ExcludesNonInteractableButton()
        {
            // Arrange
            var button = CreateButton("Button", Vector2.zero);
            button.interactable = false;
            yield return WaitForCanvasRebuild();

            // Act
            var targets = UiPointerTargets.Find();

            // Assert
            Assert.IsFalse(targets.Any(t => t.GameObject == button.gameObject));
        }

        [UnityTest]
        public IEnumerator Find_ExcludesButtonCoveredByOtherUi()
        {
            // Arrange
            var button = CreateButton("Button", Vector2.zero);
            CreateOverlay();
            yield return WaitForCanvasRebuild();

            // Act
            var targets = UiPointerTargets.Find();

            // Assert
            Assert.IsFalse(targets.Any(t => t.GameObject == button.gameObject));
        }

        [Test]
        public void GetCenter_ReturnsCenterInScreenCoordinates()
        {
            // Arrange
            var offset = new Vector2(30f, -20f);
            var button = CreateButton("Button", offset);

            // Act
            var center = UiPointerTargets.GetCenter(button.gameObject);

            // Assert
            // A Screen Space - Overlay Canvas is placed in screen pixels with a scale of 1, so its position is the
            // center of the Canvas on the screen. This is not compared with Screen.width / 2 and Screen.height / 2:
            // with an odd Game View size, the Canvas can be half a pixel off from the Screen size.
            var canvasCenter = (Vector2)_canvas.transform.position;
            Assert.AreEqual(canvasCenter.x + offset.x, center.x, 0.01f);
            Assert.AreEqual(canvasCenter.y + offset.y, center.y, 0.01f);
        }

        [Test]
        public void GetCenter_ThrowsForObjectNotUnderCanvas()
        {
            // Arrange
            var gameObject = new GameObject("PointerApiTest_NotUi");

            try
            {
                // Act & Assert
                Assert.Throws<System.ArgumentException>(() => UiPointerTargets.GetCenter(gameObject));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [UnityTest]
        public IEnumerator Click_InvokesOnClick()
        {
            // Arrange
            var button = CreateButton("Button", Vector2.zero);
            var clickCount = 0;
            button.onClick.AddListener(() => clickCount++);
            yield return WaitForCanvasRebuild();

            // Act
            yield return PointerInput.Click(button.gameObject);

            // Assert
            Assert.AreEqual(1, clickCount);
        }

        [UnityTest]
        public IEnumerator Click_InvokesOnClick_WhenEventSystemIsRecreatedWithMouseAtSamePosition()
        {
            // Arrange
            var button = CreateButton("Button", Vector2.zero);
            var clickCount = 0;
            button.onClick.AddListener(() => clickCount++);
            yield return WaitForCanvasRebuild();
            // Leave the mouse at the button's center, then recreate the EventSystem as a scene load would.
            yield return PointerInput.Click(button.gameObject);
            Object.DestroyImmediate(_eventSystem);
            _eventSystem = CreateEventSystem();
            yield return null;
            clickCount = 0;

            // Act
            yield return PointerInput.Click(button.gameObject);

            // Assert
            Assert.AreEqual(1, clickCount);
        }

        [UnityTest]
        public IEnumerator Click_DoesNotInvokeOnClickOfCoveredButton()
        {
            // Arrange
            var button = CreateButton("Button", Vector2.zero);
            var clickCount = 0;
            button.onClick.AddListener(() => clickCount++);
            CreateOverlay();
            yield return WaitForCanvasRebuild();

            // Act
            yield return PointerInput.Click(button.gameObject);

            // Assert
            Assert.AreEqual(0, clickCount);
        }

        // GraphicRaycaster ignores graphics that have not been drawn yet (Graphic.depth is -1 until the Canvas is
        // rebuilt at the end of the frame), so UI created in this frame cannot be hit until the next one.
        private static IEnumerator WaitForCanvasRebuild()
        {
            yield return null;
        }

        private static GameObject CreateEventSystem()
        {
            var gameObject = new GameObject("PointerApiTest_EventSystem", typeof(EventSystem));
            gameObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            return gameObject;
        }

        private Button CreateButton(string name, Vector2 anchoredPosition)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rectTransform = (RectTransform)gameObject.transform;
            rectTransform.SetParent(_canvas.transform, false);
            rectTransform.sizeDelta = s_buttonSize;
            rectTransform.anchoredPosition = anchoredPosition;
            return gameObject.GetComponent<Button>();
        }

        // Creates a transparent Image covering the whole Canvas in front of the other UI.
        private void CreateOverlay()
        {
            var gameObject = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
            var rectTransform = (RectTransform)gameObject.transform;
            rectTransform.SetParent(_canvas.transform, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            gameObject.GetComponent<Image>().color = Color.clear;
        }
    }
}
#endif
