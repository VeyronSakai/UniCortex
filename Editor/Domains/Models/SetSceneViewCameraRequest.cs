using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class SetSceneViewCameraRequest
    {
        public Vector3Data? position;
        public QuaternionData? rotation;
        public float? size;
        public bool? orthographic;
    }
}
