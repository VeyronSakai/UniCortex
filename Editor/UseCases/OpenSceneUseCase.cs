using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class OpenSceneUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorSceneManager _sceneManager;
        private readonly IEditorApplication _editorApplication;

        public OpenSceneUseCase(IMainThreadDispatcher dispatcher, IEditorSceneManager sceneManager,
            IEditorApplication editorApplication)
        {
            _dispatcher = dispatcher;
            _sceneManager = sceneManager;
            _editorApplication = editorApplication;
        }

        public async Task ExecuteAsync(string scenePath, CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() =>
            {
                if (_editorApplication.IsPlaying)
                {
                    throw new PlayModeException("Cannot open scene during play mode.");
                }

                // Leaving Prefab Mode with unsaved changes would ask to save them in a dialog.
                var dirtyPrefabPath = _sceneManager.GetDirtyPrefabStagePath();
                if (dirtyPrefabPath != null)
                {
                    throw new UnsavedPrefabChangesException("Cannot open a scene", dirtyPrefabPath);
                }

                // Opening a scene closes the current ones, which would drop their unsaved changes.
                var dirtyScenePaths = _sceneManager.GetDirtyScenePaths();
                if (dirtyScenePaths.Count > 0)
                {
                    throw new UnsavedSceneChangesException("Cannot open a scene", dirtyScenePaths);
                }

                _sceneManager.OpenScene(scenePath);
                return true;
            }, cancellationToken);
        }
    }
}
