using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateAssetRequest
    {
        public string type;
        public string assetPath;
    }
}
