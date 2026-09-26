using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CreateAnimationClipUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IAnimationClipOperations _operations;

        public CreateAnimationClipUseCase(IMainThreadDispatcher dispatcher, IAnimationClipOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<CreateAnimationClipResponse> ExecuteAsync(string assetPath, bool loop, float frameRate,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.Create(assetPath, loop, frameRate), cancellationToken);
        }
    }
}
