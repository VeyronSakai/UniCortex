using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetScriptableObjectPropertiesUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsProperties_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyScriptableObjectOperations
            {
                GetPropertiesResult = new GetScriptableObjectPropertiesResponse(
                    "MyNamespace.MyScriptableObject",
                    new List<SerializedPropertyEntry>
                    {
                        new SerializedPropertyEntry("m_Speed", "Float", "1.5")
                    })
            };
            var useCase = new GetScriptableObjectPropertiesUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync("Assets/Data/MyData.asset", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual("MyNamespace.MyScriptableObject", result.typeName);
            Assert.AreEqual("Assets/Data/MyData.asset", ops.LastGetPropertiesAssetPath);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.AreEqual(1, result.properties.Count);
        }
    }
}
