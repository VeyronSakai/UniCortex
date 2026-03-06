using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface ITestRunner
    {
        Task<IReadOnlyList<TestResultItem>> RunTestsAsync(RunTestsRequest request,
            CancellationToken cancellationToken);
    }
}
