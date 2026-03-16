using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class UnpauseUseCaseTest
    {
        [Test]
        public void Execute_SetsUnpauseRequested()
        {
            var cache = new EditorStateCache();
            cache.UpdatePauseState(true);
            var useCase = new UnpauseUseCase(cache);

            useCase.Execute();

            Assert.IsTrue(cache.UnpauseRequested);
        }

        [Test]
        public void ConsumeUnpauseRequest_ReturnsTrueAndClears_AfterExecute()
        {
            var cache = new EditorStateCache();
            var useCase = new UnpauseUseCase(cache);

            useCase.Execute();

            Assert.IsTrue(cache.ConsumeUnpauseRequest());
            Assert.IsFalse(cache.ConsumeUnpauseRequest());
        }
    }
}
