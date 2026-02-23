using System;

#nullable enable

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class UndoResponse
    {
        public bool success;

        public UndoResponse(bool success)
        {
            this.success = success;
        }
    }
}
