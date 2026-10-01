#nullable enable

using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class PingResponse
    {
        public string status;
        public string message;

        // Changes on every domain reload. Used to detect that a requested domain reload has completed.
        public string domainId;

        // Number of script compilations in the current domain that ended with errors (and did not reload the domain).
        public int failedCompilationCount;

        public PingResponse(string status, string message, string domainId, int failedCompilationCount)
        {
            this.status = status;
            this.message = message;
            this.domainId = domainId;
            this.failedCompilationCount = failedCompilationCount;
        }
    }
}
