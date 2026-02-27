using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class FindGameObjectsUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsResults_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyGameObjectOperations();
            ops.FindResult = new List<GameObjectBasicInfo>
            {
                new GameObjectBasicInfo("Player", 100, true)
            };
            var useCase = new FindGameObjectsUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync("Player", null, null, CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Player", result[0].name);
            Assert.AreEqual("Player", ops.LastFindName);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
