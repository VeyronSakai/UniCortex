using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetSceneViewCameraResponse
    {
        public Vector3Data position;
        public QuaternionData rotation;
        public float size;
        public bool orthographic;

        public GetSceneViewCameraResponse(
            Vector3Data position,
            QuaternionData rotation,
            float size,
            bool orthographic)
        {
            this.position = position;
            this.rotation = rotation;
            this.size = size;
            this.orthographic = orthographic;
        }
    }
}
