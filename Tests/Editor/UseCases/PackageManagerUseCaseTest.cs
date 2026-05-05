using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class PackageManagerUseCaseTest
    {
        [Test]
        public void List_DispatchesPackageManagerOperation()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPackageManagerOperations();
            operations.ListResult = new[] { CreatePackage("com.test.package") };
            var useCase = new PackageManagerUseCase(dispatcher, operations);

            var packages = useCase.ListAsync(false, true, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.AreEqual(1, operations.ListCallCount);
            Assert.IsFalse(operations.LastListOfflineMode);
            Assert.IsTrue(operations.LastListIncludeIndirectDependencies);
            Assert.AreEqual("com.test.package", packages[0].name);
        }

        [Test]
        public void Add_DispatchesPackageManagerOperation()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var operations = new SpyPackageManagerOperations();
            var useCase = new PackageManagerUseCase(dispatcher, operations);

            useCase.AddAsync("com.test.package@1.0.0", CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, dispatcher.CallCount);
            Assert.AreEqual(1, operations.AddCallCount);
            Assert.AreEqual("com.test.package@1.0.0", operations.LastAddIdentifier);
        }

        private static PackageEntry CreatePackage(string name)
        {
            return new PackageEntry(
                name,
                "Test Package",
                "1.0.0",
                $"{name}@1.0.0",
                "Registry",
                string.Empty,
                true,
                string.Empty,
                string.Empty,
                string.Empty,
                new PackageDependencyEntry[0],
                new string[0]);
        }
    }
}
