using UniCortex.Editor.Infrastructures;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class DeviceFrameRendererTest
    {
        private static readonly Color32 s_marker = new Color32(255, 0, 0, 255);
        private static readonly Color32 s_background = new Color32(0, 0, 255, 255);

        [Test]
        public void Render_PlacesScreenInsideFrame_InPortrait()
        {
            // Arrange
            var gameImage = CreateImageWithTopLeftMarker(2, 4);
            var frame = new DeviceFrame(null, new Vector4(20, 20, 20, 20), 2, 4, Vector4.zero,
                ScreenOrientation.Portrait, 0);

            // Act
            var result = DeviceFrameRenderer.Render(gameImage, frame);

            // Assert
            try
            {
                Assert.AreEqual(42, result.width);
                Assert.AreEqual(44, result.height);
                Assert.AreEqual(s_marker, GetPixelTopLeft(result, 20, 20));
                Assert.AreEqual(s_background, GetPixelTopLeft(result, 21, 23));
                // The procedural frame is drawn along the edges, and the rest of the bezel is transparent.
                Assert.AreEqual(255, GetPixelTopLeft(result, 0, 0).a);
                Assert.AreEqual(0, GetPixelTopLeft(result, 15, 15).a);
            }
            finally
            {
                Object.DestroyImmediate(gameImage);
                Object.DestroyImmediate(result);
            }
        }

        [Test]
        public void Render_KeepsLandscapeImageUpright_WhenDeviceIsRotatedClockwise()
        {
            // Arrange
            // Rotating the device clockwise by 90 degrees makes the screen LandscapeRight with auto rotation.
            var gameImage = CreateImageWithTopLeftMarker(4, 2);
            var frame = new DeviceFrame(null, new Vector4(20, 20, 20, 20), 2, 4, Vector4.zero,
                ScreenOrientation.LandscapeRight, 90);

            // Act
            var result = DeviceFrameRenderer.Render(gameImage, frame);

            // Assert
            try
            {
                Assert.AreEqual(44, result.width);
                Assert.AreEqual(42, result.height);
                Assert.AreEqual(s_marker, GetPixelTopLeft(result, 20, 20));
                Assert.AreEqual(s_background, GetPixelTopLeft(result, 23, 21));
            }
            finally
            {
                Object.DestroyImmediate(gameImage);
                Object.DestroyImmediate(result);
            }
        }

        [Test]
        public void Render_ExcludesInsetsFromScreen()
        {
            // Arrange
            var gameImage = CreateImageWithTopLeftMarker(2, 2);
            var frame = new DeviceFrame(null, new Vector4(20, 20, 20, 20), 2, 4, new Vector4(0, 0, 0, 2),
                ScreenOrientation.Portrait, 0);

            // Act
            var result = DeviceFrameRenderer.Render(gameImage, frame);

            // Assert
            try
            {
                Assert.AreEqual(s_marker, GetPixelTopLeft(result, 20, 20));
                Assert.AreEqual(s_background, GetPixelTopLeft(result, 21, 21));
                Assert.AreEqual(0, GetPixelTopLeft(result, 20, 23).a);
            }
            finally
            {
                Object.DestroyImmediate(gameImage);
                Object.DestroyImmediate(result);
            }
        }

        private static Texture2D CreateImageWithTopLeftMarker(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = s_background;
            }

            // Texture2D rows start from the bottom, so the top-left pixel is at the start of the last row.
            pixels[(height - 1) * width] = s_marker;
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static Color32 GetPixelTopLeft(Texture2D texture, int x, int y)
        {
            return texture.GetPixels32()[(texture.height - 1 - y) * texture.width + x];
        }
    }
}
