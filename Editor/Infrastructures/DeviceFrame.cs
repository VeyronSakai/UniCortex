using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    // What the Simulator view draws around the game image, in the device's portrait layout.
    internal sealed class DeviceFrame
    {
        // Frame image of the device (the "overlay" of the device definition). Null when the device has none.
        public Texture Overlay { get; }

        // Frame thickness around the screen in pixels: x = left, y = top, z = right, w = bottom.
        public Vector4 BorderSize { get; }

        // Native screen resolution of the device in portrait orientation.
        public int ScreenWidth { get; }
        public int ScreenHeight { get; }

        // Parts of the screen outside the rendering area (e.g. the Android navigation bar), in the same layout
        // as BorderSize.
        public Vector4 Insets { get; }

        public ScreenOrientation ScreenOrientation { get; }

        // Clockwise rotation of the device in degrees (0, 90, 180 or 270).
        public int Rotation { get; }

        public DeviceFrame(Texture overlay, Vector4 borderSize, int screenWidth, int screenHeight, Vector4 insets,
            ScreenOrientation screenOrientation, int rotation)
        {
            Overlay = overlay;
            BorderSize = borderSize;
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            Insets = insets;
            ScreenOrientation = screenOrientation;
            Rotation = rotation;
        }
    }
}
