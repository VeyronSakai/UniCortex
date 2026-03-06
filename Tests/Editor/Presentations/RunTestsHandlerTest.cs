using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UniCortex.Editor.Handlers.Tests;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class RunTestsHandlerTest
    {
        [Test]
        public void HandleRunTests_Returns200WithTestResults()
        {
            var spy = new SpyTestRunner(new List<TestResultItem>
            {
                new("Test1", "Passed", 0.1f), new("Test2", "Failed", 0.2f, "error"),
            });
            var useCase = new RunTestsUseCase(spy);
            var handler = new RunTestsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.TestsRun,
                "{\"testMode\":\"EditMode\",\"nameFilter\":\"\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            StringAssert.Contains("\"passed\":1", context.ResponseBody);
            StringAssert.Contains("\"failed\":1", context.ResponseBody);
            Assert.AreEqual("EditMode", spy.LastTestMode);
        }

        [Test]
        public void HandleRunTests_DefaultsToEditMode_WhenTestModeNotSpecified()
        {
            var spy = new SpyTestRunner();
            var useCase = new RunTestsUseCase(spy);
            var handler = new RunTestsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.TestsRun, "{}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual("EditMode", spy.LastTestMode);
        }

        [Test]
        public void HandleRunTests_DefaultsToEditMode_WhenBodyIsEmpty()
        {
            var spy = new SpyTestRunner();
            var useCase = new RunTestsUseCase(spy);
            var handler = new RunTestsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.TestsRun);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual("EditMode", spy.LastTestMode);
        }

        [Test]
        public void HandleRunTests_ParsesNewFilterFields()
        {
            var spy = new SpyTestRunner();
            var useCase = new RunTestsUseCase(spy);
            var handler = new RunTestsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var json = "{\"testMode\":\"EditMode\",\"nameFilter\":\"\","
                       + "\"testNames\":[\"TestA\",\"TestB\"],"
                       + "\"categoryNames\":[\"Smoke\"]}";
            var context = new FakeRequestContext("POST", ApiRoutes.TestsRun, json);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Ok, context.ResponseStatusCode);
            Assert.AreEqual(new List<string> { "TestA", "TestB" }, spy.LastRequest.testNames);
            Assert.AreEqual(new List<string> { "Smoke" }, spy.LastRequest.categoryNames);
            Assert.AreEqual(0, spy.LastRequest.groupNames?.Count ?? 0);
            Assert.AreEqual(0, spy.LastRequest.assemblyNames?.Count ?? 0);
        }
    }
}
