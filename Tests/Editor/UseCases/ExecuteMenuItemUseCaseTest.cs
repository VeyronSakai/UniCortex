using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class ExecuteMenuItemUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsExecuteMenuItem_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyUtilityOperations();
            var useCase = new ExecuteMenuItemUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync("GameObject/3D Object/Cube", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.ExecuteMenuItemCallCount);
            Assert.AreEqual("GameObject/3D Object/Cube", operations.LastMenuPath);
            Assert.IsTrue(result);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
