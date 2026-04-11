using System;
using System.ComponentModel;
using System.Threading;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Extensibility;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using NUnit.Framework;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class CustomExtensionRegistryTest
    {
        [Test]
        public void GetManifest_ReturnsCustomToolMetadata()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var registry = CustomExtensionRegistry.Discover(
                dispatcher,
                new[] { typeof(GreetingTool) });

            var manifest = registry.GetManifest();

            Assert.That(manifest.tools, Has.Length.EqualTo(1));
            Assert.That(manifest.tools[0].name, Is.EqualTo("greet_user"));
            Assert.That(manifest.tools[0].cliCommand, Is.EqualTo("greet-user"));
            Assert.That(manifest.tools[0].parameters, Has.Length.EqualTo(1));
            Assert.That(manifest.tools[0].parameters[0].name, Is.EqualTo("name"));
            Assert.That(manifest.tools[0].parameters[0].type, Is.EqualTo("string"));
            Assert.That(manifest.tools[0].parameters[0].required, Is.True);
        }

        [Test]
        public void InvokeToolAsync_DispatchesToMainThread()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var registry = CustomExtensionRegistry.Discover(
                dispatcher,
                new[] { typeof(GreetingTool) });

            var result = registry.InvokeToolAsync("greet_user", "{\"name\":\"Alice\"}", CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            Assert.That(result, Is.EqualTo("Hello, Alice!"));
            Assert.That(dispatcher.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void RegisterRoutes_RegistersCustomRoutes()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var registry = CustomExtensionRegistry.Discover(
                dispatcher,
                new[] { typeof(GreetingRoute) });
            var router = new RequestRouter();

            registry.RegisterRoutes(router);

            var context = new FakeRequestContext(HttpMethodType.Get, "/custom/greeting");
            context.SetQueryParameter("subject", "World");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(context.ResponseStatusCode, Is.EqualTo(HttpStatusCodes.Ok));
            StringAssert.Contains("Hello, World!", context.ResponseBody);
            Assert.That(dispatcher.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void Discover_SkipsReservedCliTopLevelCommand()
        {
            var dispatcher = new FakeMainThreadDispatcher();
            var registry = CustomExtensionRegistry.Discover(
                dispatcher,
                new[] { typeof(InvalidCliCommandTool) });

            var manifest = registry.GetManifest();

            Assert.That(manifest.tools, Is.Empty);
        }

        [Serializable]
        private sealed class GreetingArguments
        {
            [Description("Name to greet.")]
            [UniCortexRequired]
            public string name = string.Empty;
        }

        private sealed class GreetingTool : UniCortexCustomToolBase<GreetingArguments>
        {
            public override UniCortexCustomToolDefinition Definition { get; } = new UniCortexCustomToolDefinition(
                "greet_user",
                "Return a greeting from the Unity Editor.",
                cliCommand: "greet-user");

            protected override string Execute(GreetingArguments arguments)
            {
                return $"Hello, {arguments.name}!";
            }
        }

        private sealed class InvalidCliCommandTool : UniCortexCustomToolBase
        {
            public override UniCortexCustomToolDefinition Definition { get; } = new UniCortexCustomToolDefinition(
                "invalid_cli_tool",
                "Invalid CLI command sample.",
                cliCommand: "editor hello");

            protected override string Execute()
            {
                return "should not execute";
            }
        }

        private sealed class GreetingRoute : IUniCortexCustomRoute
        {
            public UniCortexCustomRouteDefinition Definition { get; } =
                new UniCortexCustomRouteDefinition(HttpMethodType.Get, "/custom/greeting", "Return a greeting.");

            public UniCortexCustomRouteResponse Handle(UniCortexCustomRouteRequest request)
            {
                var subject = request.GetQueryParameter("subject");
                var bodyJson = JsonUtility.ToJson(new GreetingRouteResponse($"Hello, {subject}!"));
                return new UniCortexCustomRouteResponse(HttpStatusCodes.Ok, bodyJson);
            }
        }

        [Serializable]
        private sealed class GreetingRouteResponse
        {
            public string message;

            public GreetingRouteResponse(string message)
            {
                this.message = message;
            }
        }
    }
}
