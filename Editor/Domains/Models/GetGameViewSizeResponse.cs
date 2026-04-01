using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetGameViewSizeResponse
    {
        public int screenWidth;
        public int screenHeight;

        public GetGameViewSizeResponse(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
        }
    }
}
