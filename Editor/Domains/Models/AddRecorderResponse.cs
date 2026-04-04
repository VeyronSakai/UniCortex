using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class AddRecorderResponse
    {
        public string name;

        public AddRecorderResponse(string name)
        {
            this.name = name;
        }
    }
}
