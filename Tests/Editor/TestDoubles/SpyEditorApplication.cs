using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyEditorApplication : IEditorApplication
    {
        public bool IsPlaying { get; set; }
        public bool IsPaused { get; set; }
        public int ScreenWidth { get; set; } = 800;
        public int ScreenHeight { get; set; } = 600;
        public int StepCallCount { get; private set; }

        public void Step()
        {
            StepCallCount++;
        }
    }
}
