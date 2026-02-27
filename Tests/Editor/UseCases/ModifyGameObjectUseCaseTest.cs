using System.Threading;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class ModifyGameObjectUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsModify_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            var useCase = new ModifyGameObjectUseCase(dispatcher, ops);

            useCase.ExecuteAsync(123, "NewName", true, "Player", 5, null, CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.ModifyCallCount);
            Assert.AreEqual(123, ops.LastModifyInstanceId);
            Assert.AreEqual("NewName", ops.LastModifyName);
            Assert.AreEqual(true, ops.LastModifyActiveSelf);
            Assert.AreEqual("Player", ops.LastModifyTag);
            Assert.AreEqual(5, ops.LastModifyLayer);
            Assert.IsNull(ops.LastModifyParentInstanceId);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
