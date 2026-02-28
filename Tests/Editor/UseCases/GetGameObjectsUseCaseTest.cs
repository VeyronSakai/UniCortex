using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetGameObjectsUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsResults_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            ops.GetResult = new List<GameObjectSearchResult>
            {
                new GameObjectSearchResult("Player", 100, true, "Untagged", 0, false, false,
                    new List<string> { "Transform" })
            };
            var useCase = new GetGameObjectsUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync("Player", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Player", result[0].name);
            Assert.AreEqual("Player", ops.LastGetQuery);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
