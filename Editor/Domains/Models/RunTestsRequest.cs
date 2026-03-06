#nullable enable

using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RunTestsRequest
    {
        public string testMode;
        public string nameFilter;
        public List<string>? testNames;
        public List<string>? groupNames;
        public List<string>? categoryNames;
        public List<string>? assemblyNames;

        public RunTestsRequest(
            string testMode = "EditMode",
            string nameFilter = "",
            List<string>? testNames = null,
            List<string>? groupNames = null,
            List<string>? categoryNames = null,
            List<string>? assemblyNames = null)
        {
            this.testMode = testMode;
            this.nameFilter = nameFilter;
            this.testNames = testNames;
            this.groupNames = groupNames;
            this.categoryNames = categoryNames;
            this.assemblyNames = assemblyNames;
        }
    }
}
