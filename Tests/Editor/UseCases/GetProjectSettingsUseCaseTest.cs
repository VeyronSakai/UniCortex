using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetProjectSettingsUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsSettings_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyProjectSettingsOperations
            {
                GetSettingsResult = new GetProjectSettingsResponse("Player",
                    new List<SerializedPropertyEntry>
                    {
                        new SerializedPropertyEntry("productName", "String", "MyGame")
                    })
            };
            var useCase = new GetProjectSettingsUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync("Player", CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("Player", result.category);
            Assert.AreEqual("Player", ops.LastGetSettingsCategory);
            Assert.AreEqual(1, ops.GetSettingsCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
