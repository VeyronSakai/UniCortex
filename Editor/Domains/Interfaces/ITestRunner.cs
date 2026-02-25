using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface ITestRunner
    {
        Task<IReadOnlyList<TestResultItem>> RunTestsAsync(string testMode, string nameFilter,
            CancellationToken cancellationToken);
    }
}
