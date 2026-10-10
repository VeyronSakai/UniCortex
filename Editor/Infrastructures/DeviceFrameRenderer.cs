using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    // Composes a captured game image with the device frame, the way the Simulator view draws them
    // (see UnityEditor.DeviceSimulation.DeviceView): the game image is placed on the device screen in
    // portrait layout, the frame is drawn over it, and the whole device is rotated.
    internal static class DeviceFrameRenderer
    {
        // Frame drawn for devices without a frame image, same as the Simulator view in the light skin.
        private static readonly Color32 s_proceduralFrameColor = new Color32(100, 100, 100, 255);
        private const int ProceduralFrameThickness = 10;

        // Returns a new texture. The caller is responsible for destroying it.
        public static Texture2D Render(Texture2D gameImage, DeviceFrame frame)
        {
            var screen = new Image(gameImage.GetPixels32(), gameImage.width, gameImage.height);
            screen = RotateClockwise(screen, GetQuarterTurnsToPortrait(frame.ScreenOrientation));

            var left = Mathf.RoundToInt(frame.BorderSize.x);
            var top = Mathf.RoundToInt(frame.BorderSize.y);
            var right = Mathf.RoundToInt(frame.BorderSize.z);
            var bottom = Mathf.RoundToInt(frame.BorderSize.w);
            var device = new Image(new Color32[(frame.ScreenWidth + left + right) *
                                               (frame.ScreenHeight + top + bottom)],
                frame.ScreenWidth + left + right, frame.ScreenHeight + top + bottom);

            // The game renders to the screen without the insets.
            var insetLeft = Mathf.RoundToInt(frame.Insets.x);
            var insetTop = Mathf.RoundToInt(frame.Insets.y);
            var insetRight = Mathf.RoundToInt(frame.Insets.z);
            var insetBottom = Mathf.RoundToInt(frame.Insets.w);
            DrawScaled(device, screen, new RectInt(left + insetLeft, top + insetTop,
                frame.ScreenWidth - insetLeft - insetRight, frame.ScreenHeight - insetTop - insetBottom));

            if (frame.Overlay != null)
            {
                BlendOver(device, ReadTexture(frame.Overlay, device.Width, device.Height));
            }
            else
            {
                DrawProceduralFrame(device);
            }

            var result = RotateClockwise(device, frame.Rotation / 90);
            var texture = new Texture2D(result.Width, result.Height, TextureFormat.RGBA32, false);
            texture.SetPixels32(result.Pixels);
            texture.Apply();
            return texture;
        }

        // The screen image is in the screen orientation. Returns how many clockwise quarter turns bring it to
        // the device's portrait layout, mirroring the UV mapping of DeviceView.CreateScreenMesh.
        private static int GetQuarterTurnsToPortrait(ScreenOrientation orientation)
        {
            switch (orientation)
            {
                case ScreenOrientation.LandscapeLeft:
                    return 1;
                case ScreenOrientation.PortraitUpsideDown:
                    return 2;
                case ScreenOrientation.LandscapeRight:
                    return 3;
                default:
                    return 0;
            }
        }

        private static Image RotateClockwise(Image image, int quarterTurns)
        {
            for (var turn = 0; turn < (quarterTurns % 4 + 4) % 4; turn++)
            {
                var rotated = new Image(new Color32[image.Pixels.Length], image.Height, image.Width);
                for (var y = 0; y < rotated.Height; y++)
                {
                    for (var x = 0; x < rotated.Width; x++)
                    {
                        // Rotating clockwise moves the source pixel (y, h - 1 - x) to (x, y).
                        rotated.Set(x, y, image.Get(y, image.Height - 1 - x));
                    }
                }

                image = rotated;
            }

            return image;
        }

        // Draws the source image scaled to fit the destination rect (nearest neighbor).
        // The simulated resolution can differ from the native screen resolution of the device.
        private static void DrawScaled(Image destination, Image source, RectInt rect)
        {
            if (rect.width <= 0 || rect.height <= 0)
            {
                return;
            }

            for (var y = 0; y < rect.height; y++)
            {
                var sourceY = y * source.Height / rect.height;
                for (var x = 0; x < rect.width; x++)
                {
                    var sourceX = x * source.Width / rect.width;
                    destination.Set(rect.x + x, rect.y + y, source.Get(sourceX, sourceY));
                }
            }
        }

        private static void BlendOver(Image destination, Color32[] overlay)
        {
            for (var i = 0; i < destination.Pixels.Length; i++)
            {
                var source = overlay[i];
                var target = destination.Pixels[i];
                var alpha = source.a / 255f;
                destination.Pixels[i] = new Color32(
                    (byte)Mathf.RoundToInt(source.r * alpha + target.r * (1f - alpha)),
                    (byte)Mathf.RoundToInt(source.g * alpha + target.g * (1f - alpha)),
                    (byte)Mathf.RoundToInt(source.b * alpha + target.b * (1f - alpha)),
                    (byte)Mathf.RoundToInt(source.a + target.a * (1f - alpha)));
            }
        }

        private static void DrawProceduralFrame(Image device)
        {
            for (var y = 0; y < device.Height; y++)
            {
                for (var x = 0; x < device.Width; x++)
                {
                    if (x < ProceduralFrameThickness || x >= device.Width - ProceduralFrameThickness ||
                        y < ProceduralFrameThickness || y >= device.Height - ProceduralFrameThickness)
                    {
                        device.Set(x, y, s_proceduralFrameColor);
                    }
                }
            }
        }

        // Reads a texture scaled to the given size. Frame images are usually not readable, so render them
        // into a temporary render texture and read it back.
        private static Color32[] ReadTexture(Texture source, int width, int height)
        {
            var renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var previousActive = RenderTexture.active;
            try
            {
                Graphics.Blit(source, renderTexture);
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                return texture.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTexture);
                Object.DestroyImmediate(texture);
            }
        }

        // Pixels in the layout of Texture2D (rows from the bottom), accessed with a top-left origin.
        private sealed class Image
        {
            public Color32[] Pixels { get; }
            public int Width { get; }
            public int Height { get; }

            public Image(Color32[] pixels, int width, int height)
            {
                Pixels = pixels;
                Width = width;
                Height = height;
            }

            public Color32 Get(int x, int y)
            {
                return Pixels[(Height - 1 - y) * Width + x];
            }

            public void Set(int x, int y, Color32 color)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                {
                    return;
                }

                Pixels[(Height - 1 - y) * Width + x] = color;
            }
        }
    }
}
