using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class CreateGameObjectUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsCreate_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            ops.CreateResult = new CreateGameObjectResponse("Cube", 123);
            var useCase = new CreateGameObjectUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync("NewObj", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual("Cube", result.name);
            Assert.AreEqual(123, result.instanceId);
            Assert.AreEqual("NewObj", ops.LastCreateName);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
