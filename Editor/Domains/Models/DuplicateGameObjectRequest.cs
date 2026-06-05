using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class DuplicateGameObjectRequest
    {
        public int instanceId;
        public string? name;
    }
}
