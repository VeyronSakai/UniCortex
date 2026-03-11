#nullable enable

using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class RunTestsResponse
    {
        public int passed;
        public int failed;
        public int skipped;
        public List<TestResultEntry> results;

        public RunTestsResponse(int passed, int failed, int skipped, List<TestResultEntry> results)
        {
            this.passed = passed;
            this.failed = failed;
            this.skipped = skipped;
            this.results = results;
        }
    }
}
