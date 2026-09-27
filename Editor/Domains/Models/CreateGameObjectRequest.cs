using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CreateGameObjectRequest
    {
        public string name;
        public int? parentInstanceId;
        public int? siblingIndex;
        public bool? useRectTransform;
    }
}
