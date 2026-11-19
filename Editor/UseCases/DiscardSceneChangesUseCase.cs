using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class DiscardSceneChangesUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IEditorSceneManager _sceneManager;
        private readonly IEditorApplication _editorApplication;

        public DiscardSceneChangesUseCase(IMainThreadDispatcher dispatcher, IEditorSceneManager sceneManager,
            IEditorApplication editorApplication)
        {
            _dispatcher = dispatcher;
            _sceneManager = sceneManager;
            _editorApplication = editorApplication;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() =>
            {
                if (_editorApplication.IsPlaying)
                {
                    throw new PlayModeException("Cannot discard scene changes during play mode.");
                }

                // An untitled scene has no saved state to go back to. Fail before discarding anything, so that
                // the changes are either all discarded or all kept.
                if (_sceneManager.GetDirtyScenePaths().Any(string.IsNullOrEmpty))
                {
                    throw new InvalidOperationException(
                        "Cannot discard the changes of an untitled scene, because it has no saved state to go " +
                        "back to. Save it to an asset path with save_scene_as (CLI: scene save-as) instead.");
                }

                _sceneManager.DiscardChanges();
            }, cancellationToken);
        }
    }
}
