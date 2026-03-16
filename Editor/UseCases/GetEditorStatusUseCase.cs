using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;

namespace UniCortex.Editor.UseCases
{
    // Reads from EditorStateCache (updated by main-thread callbacks) so it
    // responds immediately even when the editor is paused.
    internal sealed class GetEditorStatusUseCase
    {
        private readonly EditorStateCache _stateCache;

        public GetEditorStatusUseCase(EditorStateCache stateCache)
        {
            _stateCache = stateCache;
        }

        public EditorStatusResponse Execute()
        {
            return new EditorStatusResponse(_stateCache.IsPlaying, _stateCache.IsPaused);
        }
    }
}
