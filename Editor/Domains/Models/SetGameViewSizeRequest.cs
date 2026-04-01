using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetGameViewSizeRequest
    {
        public int index = -1;
        public int width;
        public int height;
    }
}
