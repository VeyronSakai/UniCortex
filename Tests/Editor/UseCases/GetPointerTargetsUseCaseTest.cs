using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetPointerTargetsUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsPointerTargets_And_DispatchesToMainThread()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var target = SpyPointerTargetOperations.CreateTarget(100f, 200f, false, "");
            var ops = new SpyPointerTargetOperations
            {
                PointerTargetsToReturn = new List<PointerTarget> { target }
            };
            var useCase = new GetPointerTargetsUseCase(dispatcher, ops);

            // Act
            var result = useCase.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual(1, ops.GetPointerTargetsCallCount);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.AreEqual(1, result.Count);
            Assert.AreSame(target, result[0]);
        }
    }
}
