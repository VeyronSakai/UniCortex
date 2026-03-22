using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CaptureGameViewResponse
    {
        public string pngDataBase64;

        public CaptureGameViewResponse(string pngDataBase64)
        {
            this.pngDataBase64 = pngDataBase64;
        }
    }
}
