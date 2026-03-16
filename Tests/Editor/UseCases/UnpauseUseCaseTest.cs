using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class UnpauseUseCaseTest
    {
        [Test]
        public void ExecuteAsync_SetsIsPausedToFalse()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var editorApplication = new SpyEditorApplication { IsPaused = true };
            var useCase = new UnpauseUseCase(dispatcher, editorApplication);

            useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(editorApplication.IsPaused);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
