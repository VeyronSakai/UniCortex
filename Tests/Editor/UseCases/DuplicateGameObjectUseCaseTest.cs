using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class DuplicateGameObjectUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsDuplicate_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations
            {
                DuplicateResult = new DuplicateGameObjectResponse("Cube (1)", 789)
            };
            var useCase = new DuplicateGameObjectUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync(456, "Custom", CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.DuplicateCallCount);
            Assert.AreEqual(456, ops.LastDuplicateInstanceId);
            Assert.AreEqual("Custom", ops.LastDuplicateName);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.AreEqual("Cube (1)", result.name);
            Assert.AreEqual(789, result.instanceId);
        }
    }
}
