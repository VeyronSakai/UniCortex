using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetUIPointerTargetsUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IUIPointerTargetOperations _operations;

        public GetUIPointerTargetsUseCase(IMainThreadDispatcher dispatcher, IUIPointerTargetOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<List<UIPointerTargetEntry>> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetUIPointerTargetsAsync(cancellationToken), cancellationToken);
            return await task;
        }
    }
}
