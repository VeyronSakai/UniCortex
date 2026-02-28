using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetSceneHierarchyUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorSceneManager _sceneManager;

        public GetSceneHierarchyUseCase(IMainThreadDispatcher dispatcher, IEditorSceneManager sceneManager)
        {
            _dispatcher = dispatcher;
            _sceneManager = sceneManager;
        }

        public async Task<SceneHierarchyResponse> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(() => _sceneManager.GetHierarchy(), cancellationToken);
        }
    }
}
