using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetPointerTargetsUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPointerTargetOperations _operations;

        public GetPointerTargetsUseCase(IMainThreadDispatcher dispatcher, IPointerTargetOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<List<PointerTarget>> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetPointerTargetsAsync(cancellationToken), cancellationToken);
            return await task;
        }
    }
}
