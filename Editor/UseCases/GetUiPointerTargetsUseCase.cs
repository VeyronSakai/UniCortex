using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetUiPointerTargetsUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IUiPointerTargetOperations _operations;

        public GetUiPointerTargetsUseCase(IMainThreadDispatcher dispatcher, IUiPointerTargetOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<List<UiPointerTargetEntry>> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetUiPointerTargetsAsync(cancellationToken), cancellationToken);
            return await task;
        }
    }
}
