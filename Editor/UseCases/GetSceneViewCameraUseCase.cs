using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetSceneViewCameraUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorWindowOperations _operations;

        public GetSceneViewCameraUseCase(IMainThreadDispatcher dispatcher, IEditorWindowOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<GetSceneViewCameraResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.GetSceneViewCamera(), cancellationToken);
        }
    }
}
