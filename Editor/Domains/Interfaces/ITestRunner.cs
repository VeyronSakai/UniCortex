using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface ITestRunner
    {
        // Starts a test run and returns once it has started. The results are stored in ITestResultStore when it finishes.
        Task StartAsync(RunTestsRequest request, CancellationToken cancellationToken);
    }
}
