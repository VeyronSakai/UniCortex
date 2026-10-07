using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetUIPointerTargetsUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsUIPointerTargets_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var target = new UIPointerTargetEntry("Canvas/Button", 100, new ScreenRect(50f, 180f, 100f, 40f));
            var ops = new SpyUIPointerTargetOperations
            {
                UIPointerTargetsToReturn = new List<UIPointerTargetEntry> { target }
            };
            var useCase = new GetUIPointerTargetsUseCase(dispatcher, ops);

            // Act
            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, ops.GetUIPointerTargetsCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(target, result[0]);
        }
    }
}
