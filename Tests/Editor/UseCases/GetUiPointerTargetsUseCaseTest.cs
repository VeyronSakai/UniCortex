using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetUiPointerTargetsUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsUiPointerTargets_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var target = new UiPointerTargetEntry("Canvas/Button", 100, new ScreenRect(50f, 180f, 100f, 40f));
            var ops = new SpyUiPointerTargetOperations
            {
                UiPointerTargetsToReturn = new List<UiPointerTargetEntry> { target }
            };
            var useCase = new GetUiPointerTargetsUseCase(dispatcher, ops);

            // Act
            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, ops.GetUiPointerTargetsCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(target, result[0]);
        }
    }
}
