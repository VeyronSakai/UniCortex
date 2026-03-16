using UniCortex.Editor.Infrastructures;

namespace UniCortex.Editor.UseCases
{
    // Requests pause via EditorStateCache. The actual EditorApplication.isPaused = true
    // is applied on the main thread by EntryPoint's update callback.
    internal sealed class PauseUseCase
    {
        private readonly EditorStateCache _stateCache;

        public PauseUseCase(EditorStateCache stateCache)
        {
            _stateCache = stateCache;
        }

        public void Execute()
        {
            _stateCache.RequestPause();
        }
    }
}
