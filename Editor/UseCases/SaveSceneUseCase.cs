using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SaveSceneUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorSceneManager _sceneManager;

        public SaveSceneUseCase(IMainThreadDispatcher dispatcher, IEditorSceneManager sceneManager)
        {
            _dispatcher = dispatcher;
            _sceneManager = sceneManager;
        }

        public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(() => _sceneManager.SaveOpenScenes(), cancellationToken);
        }
    }
}
