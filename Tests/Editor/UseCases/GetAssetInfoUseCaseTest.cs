using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class GetAssetInfoUseCaseTest
    {
        [Test]
        public void ExecuteAsync_ReturnsInfo_And_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyAssetOperations
            {
                GetInfoResult = new AssetInfoResponse("Assets/Test.mat", "Material",
                    new List<SerializedPropertyEntry>
                    {
                        new SerializedPropertyEntry("m_Name", "String", "Test")
                    })
            };
            var useCase = new GetAssetInfoUseCase(dispatcher, operations);

            var result = useCase.ExecuteAsync("Assets/Test.mat", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, operations.GetInfoCallCount);
            Assert.AreEqual("Assets/Test.mat", operations.LastGetInfoAssetPath);
            Assert.AreEqual("Material", result.type);
            Assert.AreEqual(1, result.properties.Count);
            Assert.AreEqual(1, dispatcher.CallCount);
        }
    }
}
