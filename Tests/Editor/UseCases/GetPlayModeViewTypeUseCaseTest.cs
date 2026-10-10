using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetPlayModeViewTypeUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsViewType_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPlayModeViewOperations { ViewType = PlayModeViewTypes.SimulatorView };
            var useCase = new GetPlayModeViewTypeUseCase(dispatcher, operations);

            // Act
            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(PlayModeViewTypes.SimulatorView, result.viewType);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
