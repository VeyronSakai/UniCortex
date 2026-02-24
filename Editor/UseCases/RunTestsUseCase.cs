using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class RunTestsUseCase
    {
        private readonly ITestRunner _testRunner;

        public RunTestsUseCase(ITestRunner testRunner)
        {
            _testRunner = testRunner;
        }

        public async Task<RunTestsResponse> ExecuteAsync(string testMode, string nameFilter,
            CancellationToken cancellationToken)
        {
            var items = await _testRunner.RunTestsAsync(testMode, nameFilter, cancellationToken);

            var passed = items.Count(i => i.Status == "Passed");
            var failed = items.Count(i => i.Status == "Failed");
            var skipped = items.Count(i => i.Status != "Passed" && i.Status != "Failed");

            var results = new List<TestResultEntry>(items.Count);
            foreach (var item in items)
            {
                results.Add(new TestResultEntry(item.Name, item.Status, item.Duration, item.Message));
            }

            return new RunTestsResponse(passed, failed, skipped, results);
        }
    }
}
