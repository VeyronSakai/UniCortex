using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CaptureSceneViewResponse
    {
        public string pngDataBase64;

        public CaptureSceneViewResponse(string pngDataBase64)
        {
            this.pngDataBase64 = pngDataBase64;
        }
    }
}
