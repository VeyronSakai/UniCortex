using UniCortex.Editor.Infrastructures;

namespace UniCortex.Editor.UseCases
{
    // Requests unpause via EditorStateCache. The actual EditorApplication.isPaused = false
    // is applied on the main thread by EntryPoint's update callback.
    internal sealed class UnpauseUseCase
    {
        private readonly EditorStateCache _stateCache;

        public UnpauseUseCase(EditorStateCache stateCache)
        {
            _stateCache = stateCache;
        }

        public void Execute()
        {
            _stateCache.RequestUnpause();
        }
    }
}
