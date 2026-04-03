using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class SaveUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsSave_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var editorApp = new SpyEditorApplication();
            var useCase = new SaveUseCase(dispatcher, editorApp);

            useCase.ExecuteAsync(CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, editorApp.SaveCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
