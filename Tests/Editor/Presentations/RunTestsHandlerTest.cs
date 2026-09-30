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
        public void HandleRunTests_Returns202_WhenRunStarted()
        {
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication();
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);
            var handler = new RunTestsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TestsRun,
                "{\"testMode\":\"EditMode\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Accepted, context.ResponseStatusCode);
            StringAssert.Contains("\"success\":true", context.ResponseBody);
            Assert.AreEqual(1, spy.StartCallCount);
            Assert.AreEqual(TestModes.EditMode, spy.LastTestMode);
        }

        [Test]
        public void HandleRunTests_DefaultsToEditMode_WhenTestModeNotSpecified()
        {
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication();
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);
            var handler = new RunTestsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TestsRun, "{}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Accepted, context.ResponseStatusCode);
            Assert.AreEqual(TestModes.EditMode, spy.LastTestMode);
        }

        [Test]
        public void HandleRunTests_DefaultsToEditMode_WhenBodyIsEmpty()
        {
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication();
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);
            var handler = new RunTestsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TestsRun);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Accepted, context.ResponseStatusCode);
            Assert.AreEqual(TestModes.EditMode, spy.LastTestMode);
        }

        [Test]
        public void HandleRunTests_ParsesNewFilterFields()
        {
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication();
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);
            var handler = new RunTestsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var json = "{\"testMode\":\"EditMode\","
                       + "\"testNames\":[\"TestA\",\"TestB\"],"
                       + "\"categoryNames\":[\"Smoke\"]}";
            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TestsRun, json);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.Accepted, context.ResponseStatusCode);
            Assert.AreEqual(new List<string> { "TestA", "TestB" }, spy.LastRequest.testNames);
            Assert.AreEqual(new List<string> { "Smoke" }, spy.LastRequest.categoryNames);
            Assert.AreEqual(0, spy.LastRequest.groupNames?.Count ?? 0);
            Assert.AreEqual(0, spy.LastRequest.assemblyNames?.Count ?? 0);
        }

        [Test]
        public void HandleRunTests_Returns400_WhenInPlayMode()
        {
            var spy = new SpyTestRunner();
            var editorApp = new SpyEditorApplication { IsPlaying = true };
            var dispatcher = new FakeMainThreadDispatcher();
            var useCase = new RunTestsUseCase(spy, dispatcher, editorApp);
            var handler = new RunTestsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext(HttpMethodType.Post, ApiRoutes.TestsRun,
                "{\"testMode\":\"EditMode\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(HttpStatusCodes.BadRequest, context.ResponseStatusCode);
            StringAssert.Contains("Cannot run tests during play mode", context.ResponseBody);
            Assert.AreEqual(0, spy.StartCallCount);
        }
    }
}
