using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetSceneViewCameraResponse
    {
        public bool success;

        public SetSceneViewCameraResponse(bool success)
        {
            this.success = success;
        }
    }
}
