#nullable enable

using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RunTestsRequest
    {
        public string testMode;
        public string nameFilter;

        public RunTestsRequest(string testMode = "EditMode", string nameFilter = "")
        {
            this.testMode = testMode;
            this.nameFilter = nameFilter;
        }
    }
}
