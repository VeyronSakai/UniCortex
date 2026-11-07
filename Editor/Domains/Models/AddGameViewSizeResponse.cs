using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddGameViewSizeResponse
    {
        public int index;

        // false when an entry with the same name and size already existed, which is returned instead.
        public bool added;

        public AddGameViewSizeResponse(int index, bool added)
        {
            this.index = index;
            this.added = added;
        }
    }
}
