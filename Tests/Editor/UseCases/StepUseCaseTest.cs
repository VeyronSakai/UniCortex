using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class StepUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsStep_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var editorApplication = new SpyEditorApplication();
            var useCase = new StepUseCase(dispatcher, editorApplication);

            useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, editorApplication.StepCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
