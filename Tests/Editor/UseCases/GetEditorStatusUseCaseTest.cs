using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetEditorStatusUseCaseTest
    {
        [Test]
        public void Execute_WhenPlayingAndPaused_ReturnsCorrectStatus()
        {
            var cache = new EditorStateCache();
            cache.UpdatePlayModeState(true);
            cache.UpdatePauseState(true);
            var useCase = new GetEditorStatusUseCase(cache);

            var result = useCase.Execute();

            Assert.IsTrue(result.isPlaying);
            Assert.IsTrue(result.isPaused);
        }

        [Test]
        public void Execute_WhenNotPlayingAndNotPaused_ReturnsCorrectStatus()
        {
            var cache = new EditorStateCache();
            var useCase = new GetEditorStatusUseCase(cache);

            var result = useCase.Execute();

            Assert.IsFalse(result.isPlaying);
            Assert.IsFalse(result.isPaused);
        }

        [Test]
        public void Execute_WhenPlayingAndNotPaused_ReturnsCorrectStatus()
        {
            var cache = new EditorStateCache();
            cache.UpdatePlayModeState(true);
            var useCase = new GetEditorStatusUseCase(cache);

            var result = useCase.Execute();

            Assert.IsTrue(result.isPlaying);
            Assert.IsFalse(result.isPaused);
        }
    }
}
