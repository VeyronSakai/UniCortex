using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetSceneViewCameraUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorWindowOperations _operations;

        public SetSceneViewCameraUseCase(IMainThreadDispatcher dispatcher, IEditorWindowOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<SetSceneViewCameraResponse> ExecuteAsync(
            SetSceneViewCameraRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentException("Request body is required.", nameof(request));
            }

            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.SetSceneViewCamera(request), cancellationToken);
            return new SetSceneViewCameraResponse(true);
        }
    }
}
