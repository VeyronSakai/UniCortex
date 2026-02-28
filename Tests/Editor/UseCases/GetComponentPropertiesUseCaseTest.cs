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
            ops.GetPropertiesResult = new ComponentPropertiesResponse("UnityEngine.Transform",
                new List<SerializedPropertyEntry>
                {
                    new SerializedPropertyEntry("m_LocalPosition", "Vector3", "(0, 0, 0)")
                });
            var useCase = new GetComponentPropertiesUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync(123, "UnityEngine.Transform", 0, CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual("UnityEngine.Transform", result.componentType);
            Assert.AreEqual(123, ops.LastGetPropertiesInstanceId);
            Assert.AreEqual("UnityEngine.Transform", ops.LastGetPropertiesComponentType);
            Assert.AreEqual(0, ops.LastGetPropertiesComponentIndex);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
