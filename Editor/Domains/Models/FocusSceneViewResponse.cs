using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class FocusSceneViewResponse
    {
        public bool success;

        public FocusSceneViewResponse(bool success)
        {
            this.success = success;
        }
    }
}
