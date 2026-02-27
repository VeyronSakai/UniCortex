using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetGameObjectInfoUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsInfo_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            ops.GetInfoResult = new GameObjectInfoResponse("Cube", 123, true, "Untagged", 0,
                new List<ComponentInfoEntry> { new ComponentInfoEntry("Transform", 0) });
            var useCase = new GetGameObjectInfoUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync(123, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("Cube", result.name);
            Assert.AreEqual(123, ops.LastGetInfoInstanceId);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
