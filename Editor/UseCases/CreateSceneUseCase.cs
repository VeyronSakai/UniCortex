using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CreateSceneUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorSceneManager _sceneManager;
        private readonly IEditorApplication _editorApplication;

        public CreateSceneUseCase(IMainThreadDispatcher dispatcher, IEditorSceneManager sceneManager,
            IEditorApplication editorApplication)
        {
            _dispatcher = dispatcher;
            _sceneManager = sceneManager;
            _editorApplication = editorApplication;
        }

        public async Task<bool> ExecuteAsync(string scenePath, CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(() =>
            {
                if (_editorApplication.IsPlaying)
                {
                    throw new PlayModeException("Cannot create scene during play mode.");
                }

                // Leaving Prefab Mode with unsaved changes would ask to save them in a dialog.
                var dirtyPrefabPath = _sceneManager.GetDirtyPrefabStagePath();
                if (dirtyPrefabPath != null)
                {
                    throw new UnsavedPrefabChangesException("Cannot create a scene", dirtyPrefabPath);
                }

                // The new scene replaces the current ones, which would drop their unsaved changes.
                var dirtyScenePaths = _sceneManager.GetDirtyScenePaths();
                if (dirtyScenePaths.Count > 0)
                {
                    throw new UnsavedSceneChangesException("Cannot create a scene", dirtyScenePaths);
                }

                return _sceneManager.CreateScene(scenePath);
            }, cancellationToken);
        }
    }
}
