using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetGameViewSizeListResponse
    {
        public GameViewSizeEntry[] sizes = Array.Empty<GameViewSizeEntry>();
        public int selectedIndex;
    }
}
