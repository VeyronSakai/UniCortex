using System;

namespace UniCortex.Editor.Domains.Models
{
    // Response of POST /tests/run. The run's results are obtained from GET /tests/result.
    [Serializable]
    public class RunTestsAcceptedResponse
    {
        public bool success;

        public RunTestsAcceptedResponse(bool success)
        {
            this.success = success;
        }
    }
}
