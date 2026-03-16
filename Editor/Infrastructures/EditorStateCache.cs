namespace UniCortex.Editor.Infrastructures
{
    // Thread-safe cache for EditorApplication.isPlaying / isPaused.
    // Updated from main-thread event callbacks so HTTP threads can read
    // without calling Unity API directly (which may block on non-main threads).
    internal sealed class EditorStateCache
    {
        private volatile bool _isPlaying;
        private volatile bool _isPaused;
        private volatile bool _pauseRequested;

        public bool IsPlaying => _isPlaying;
        public bool IsPaused => _isPaused;
        public bool PauseRequested => _pauseRequested;

        public void UpdatePlayModeState(bool isPlaying)
        {
            _isPlaying = isPlaying;
        }

        public void UpdatePauseState(bool isPaused)
        {
            _isPaused = isPaused;
        }

        private volatile bool _unpauseRequested;

        public bool UnpauseRequested => _unpauseRequested;

        public void RequestPause()
        {
            _pauseRequested = true;
        }

        public bool ConsumePauseRequest()
        {
            if (!_pauseRequested) return false;
            _pauseRequested = false;
            return true;
        }

        public void RequestUnpause()
        {
            _unpauseRequested = true;
        }

        public bool ConsumeUnpauseRequest()
        {
            if (!_unpauseRequested) return false;
            _unpauseRequested = false;
            return true;
        }
    }
}
