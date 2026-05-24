using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class CreateScriptableObjectUseCaseTest
    {
        [Test]
        public void ExecuteAsync_CallsCreate_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var ops = new SpyScriptableObjectOperations
            {
                CreateResult = new CreateScriptableObjectResponse(true, 42)
            };
            var useCase = new CreateScriptableObjectUseCase(dispatcher, ops);

            var result = useCase.ExecuteAsync("MyNamespace.MyScriptableObject", "Assets/Data/MyData.asset",
                CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, ops.CreateCallCount);
            Assert.AreEqual("MyNamespace.MyScriptableObject", ops.LastCreateTypeName);
            Assert.AreEqual("Assets/Data/MyData.asset", ops.LastCreateAssetPath);
            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.IsTrue(result.success);
            Assert.AreEqual(42, result.instanceId);
        }
    }
}
