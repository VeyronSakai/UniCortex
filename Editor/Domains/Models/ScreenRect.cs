using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ScreenRect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public ScreenRect(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }
}
