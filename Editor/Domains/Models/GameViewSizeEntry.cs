using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GameViewSizeEntry
    {
        public int index;
        public string name = "";
        public int width;
        public int height;
        public string sizeType = "";
    }
}
