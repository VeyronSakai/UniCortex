using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class ClosePrefabUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPrefabOperations _operations;
        private readonly IEditorSceneManager _sceneManager;

        public ClosePrefabUseCase(IMainThreadDispatcher dispatcher, IPrefabOperations operations,
            IEditorSceneManager sceneManager)
        {
            _dispatcher = dispatcher;
            _operations = operations;
            _sceneManager = sceneManager;
        }

        public async Task ExecuteAsync(bool discardChanges, CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() =>
            {
                var dirtyPrefabPath = _sceneManager.GetDirtyPrefabStagePath();
                if (dirtyPrefabPath != null && !discardChanges)
                {
                    throw new UnsavedPrefabChangesException("Cannot close Prefab Mode", dirtyPrefabPath);
                }

                _operations.ClosePrefab(discardChanges);
            }, cancellationToken);
        }
    }
}
