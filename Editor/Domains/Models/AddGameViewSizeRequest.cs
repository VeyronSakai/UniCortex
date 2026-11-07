using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddGameViewSizeRequest
    {
        public string name = "";
        public int width;
        public int height;
    }
}
