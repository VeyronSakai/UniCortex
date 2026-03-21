using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CaptureScreenshotResponse
    {
        public string pngDataBase64;

        public CaptureScreenshotResponse(string pngDataBase64)
        {
            this.pngDataBase64 = pngDataBase64;
        }
    }
}
