using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class OpenPrefabUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPrefabOperations _operations;
        private readonly IEditorSceneManager _sceneManager;

        public OpenPrefabUseCase(IMainThreadDispatcher dispatcher, IPrefabOperations operations,
            IEditorSceneManager sceneManager)
        {
            _dispatcher = dispatcher;
            _operations = operations;
            _sceneManager = sceneManager;
        }

        public async Task ExecuteAsync(string assetPath, CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() =>
            {
                // Opening a Prefab, even the one already open, leaves the current Prefab Mode first.
                var dirtyPrefabPath = _sceneManager.GetDirtyPrefabStagePath();
                if (dirtyPrefabPath != null)
                {
                    throw new UnsavedPrefabChangesException("Cannot open a Prefab", dirtyPrefabPath);
                }

                _operations.OpenPrefab(assetPath);
            }, cancellationToken);
        }
    }
}
