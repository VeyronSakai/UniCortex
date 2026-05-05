using System.Threading;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Handlers.PackageManager;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class PackageManagerHandlerTest
    {
        private SpyPackageManagerOperations _operations;
        private RequestRouter _router;

        [SetUp]
        public void SetUp()
        {
            _operations = new SpyPackageManagerOperations();
            var useCase = new PackageManagerUseCase(new FakeMainThreadDispatcher(), _operations);
            var handler = new PackageManagerHandler(useCase);
            _router = new RequestRouter();
            handler.Register(_router);
        }

        [Test]
        public void List_Returns200_WithPackageEntries()
        {
            _operations.ListResult = new[]
            {
                CreatePackage("com.test.package", "1.0.0")
            };
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.PackageManagerList);
            context.SetQueryParameter("offlineMode", "false");
            context.SetQueryParameter("includeIndirectDependencies", "true");

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.IsFalse(_operations.LastListOfflineMode);
            Assert.IsTrue(_operations.LastListIncludeIndirectDependencies);
            var response = JsonUtility.FromJson<PackageManagerPackagesResponse>(context.ResponseBody);
            Assert.AreEqual(1, response.packages.Length);
            Assert.AreEqual("com.test.package", response.packages[0].name);
            Assert.AreEqual("1.0.0", response.packages[0].version);
        }

        [Test]
        public void Search_MissingPackageIdOrName_Returns400()
        {
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.PackageManagerSearch);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.That(context.ResponseBody, Does.Contain("packageIdOrName is required"));
        }

        [Test]
        public void Search_Returns200_WithPackageEntries()
        {
            _operations.SearchResult = new[]
            {
                CreatePackage("com.test.search", "2.0.0")
            };
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.PackageManagerSearch);
            context.SetQueryParameter("packageIdOrName", "com.test.search");
            context.SetQueryParameter("offlineMode", "true");

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual("com.test.search", _operations.LastSearchPackageIdOrName);
            Assert.IsTrue(_operations.LastSearchOfflineMode);
            var response = JsonUtility.FromJson<PackageManagerPackagesResponse>(context.ResponseBody);
            Assert.AreEqual("com.test.search", response.packages[0].name);
        }

        [Test]
        public void Add_EmptyBody_Returns400()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PackageManagerAdd);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.That(context.ResponseBody, Does.Contain("identifier is required"));
        }

        [Test]
        public void Add_MissingIdentifier_Returns400()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PackageManagerAdd, "{}");

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.That(context.ResponseBody, Does.Contain("identifier is required"));
        }

        [Test]
        public void Add_Returns200_WithPackageEntry()
        {
            _operations.AddResult = CreatePackage("com.test.added", "3.0.0");
            var context = new FakeRequestContext(
                HttpMethodType.Post,
                ApiRoutes.PackageManagerAdd,
                "{\"identifier\":\"com.test.added@3.0.0\"}");

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual("com.test.added@3.0.0", _operations.LastAddIdentifier);
            var response = JsonUtility.FromJson<PackageEntry>(context.ResponseBody);
            Assert.AreEqual("com.test.added", response.name);
            Assert.AreEqual("3.0.0", response.version);
        }

        [Test]
        public void Remove_MissingPackageName_Returns400()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PackageManagerRemove, "{}");

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.That(context.ResponseBody, Does.Contain("packageName is required"));
        }

        [Test]
        public void Remove_Returns200_WhenOperationSucceeds()
        {
            var context = new FakeRequestContext(
                HttpMethodType.Post,
                ApiRoutes.PackageManagerRemove,
                "{\"packageName\":\"com.test.package\"}");

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual("com.test.package", _operations.LastRemovePackageName);
            var response = JsonUtility.FromJson<PackageManagerRemoveResponse>(context.ResponseBody);
            Assert.IsTrue(response.success);
        }

        [Test]
        public void Resolve_Returns200_WhenOperationSucceeds()
        {
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.PackageManagerResolve);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(1, _operations.ResolveCallCount);
            var response = JsonUtility.FromJson<PackageManagerResolveResponse>(context.ResponseBody);
            Assert.IsTrue(response.success);
        }

        [Test]
        public void PackageManagerFailure_Returns400_WithUnityErrorMessage()
        {
            _operations.ExceptionToThrow = new PackageManagerOperationException("Package not found.");
            var context = new FakeRequestContext(HttpMethodType.Get, ApiRoutes.PackageManagerList);

            _router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            Assert.That(context.ResponseBody, Does.Contain("Package not found."));
        }

        private static PackageEntry CreatePackage(string name, string version)
        {
            return new PackageEntry(
                name,
                "Test Package",
                version,
                $"{name}@{version}",
                "Registry",
                string.Empty,
                true,
                "/tmp/package",
                $"Packages/{name}",
                "A test package.",
                new[] { new PackageDependencyEntry("com.test.dependency", "1.0.0") },
                new string[0]);
        }
    }
}
