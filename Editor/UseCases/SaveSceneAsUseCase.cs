using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SaveSceneAsUseCase
    {
        private const string SceneExtension = ".unity";

        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorSceneManager _sceneManager;
        private readonly IEditorApplication _editorApplication;

        public SaveSceneAsUseCase(IMainThreadDispatcher dispatcher, IEditorSceneManager sceneManager,
            IEditorApplication editorApplication)
        {
            _dispatcher = dispatcher;
            _sceneManager = sceneManager;
            _editorApplication = editorApplication;
        }

        public async Task ExecuteAsync(string scenePath, CancellationToken cancellationToken = default)
        {
            if (!scenePath.EndsWith(SceneExtension, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Scene path must end with '{SceneExtension}' (got '{scenePath}').");
            }

            await _dispatcher.RunOnMainThreadAsync(() =>
            {
                if (_editorApplication.IsPlaying)
                {
                    throw new PlayModeException("Cannot save a scene during play mode.");
                }

                _sceneManager.SaveActiveSceneAs(scenePath);
            }, cancellationToken);
        }
    }
}
