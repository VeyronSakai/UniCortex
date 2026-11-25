using System;
using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetComponentPropertiesUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsProperties_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            ops.GetPropertiesResult = new GetComponentPropertiesResponse("UnityEngine.Transform",
                new List<SerializedPropertyEntry>
                {
                    new SerializedPropertyEntry("m_LocalPosition", "Vector3", "(0, 0, 0)")
                });
            var useCase = new GetComponentPropertiesUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync(123, "UnityEngine.Transform", "UnityEngine.CoreModule", 0,
                cancellationToken: CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("UnityEngine.Transform", result.componentType);
            Assert.AreEqual(123, ops.LastGetPropertiesInstanceId);
            Assert.AreEqual("UnityEngine.Transform", ops.LastGetPropertiesComponentType);
            Assert.AreEqual("UnityEngine.CoreModule", ops.LastGetPropertiesAssembly);
            Assert.AreEqual(0, ops.LastGetPropertiesComponentIndex);
            Assert.IsNull(ops.LastGetPropertiesPropertyPath);
            Assert.IsNull(ops.LastGetPropertiesDepth);
            Assert.AreEqual(1, dispatcher.CallCount);
        }

        [Test]
        public void ExecuteAsync_PassesPropertyPathAndDepth()
        {
            // Arrange
            var ops = new SpyComponentOperations();
            var useCase = new GetComponentPropertiesUseCase(new FakeMainThreadDispatcher(), ops);

            // Act
            useCase.ExecuteAsync(123, "UnityEngine.Playables.PlayableDirector", "UnityEngine.DirectorModule", 0,
                "m_SceneBindings", 2, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            Assert.AreEqual("m_SceneBindings", ops.LastGetPropertiesPropertyPath);
            Assert.AreEqual(2, ops.LastGetPropertiesDepth);
        }

        [Test]
        public void ExecuteAsync_WithNegativeDepth_ThrowsArgumentException_WithoutDispatching()
        {
            // Arrange
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyComponentOperations();
            var useCase = new GetComponentPropertiesUseCase(dispatcher, ops);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => useCase.ExecuteAsync(123, "UnityEngine.Transform",
                "UnityEngine.CoreModule", 0, null, -1, CancellationToken.None).GetAwaiter().GetResult());

            // Assert
            StringAssert.Contains("depth must be 0 or greater", ex.Message);
            Assert.AreEqual(0, ops.GetPropertiesCallCount);
            Assert.AreEqual(0, dispatcher.CallCount);
        }
    }
}
