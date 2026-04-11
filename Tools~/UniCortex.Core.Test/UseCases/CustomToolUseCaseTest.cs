using NUnit.Framework;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.UseCases;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class CustomToolUseCaseTest
{
    [Test]
    public async ValueTask GetManifest_ReturnsManifestFromUnityEditor()
    {
        var client = new FakeUnityEditorClient
        {
            GetResponse = new GetCustomToolsManifestResponse(
                [
                    new CustomToolManifestEntry(
                        "greet_user",
                        "Return a greeting.",
                        "greet-user",
                        exposeToMcp: true,
                        exposeToCli: true,
                        parameters:
                        [
                            new CustomToolParameterDefinition("name", "string", required: true)
                        ])
                ])
        };
        var useCase = new CustomToolUseCase(client);

        var manifest = await useCase.GetManifestAsync(CancellationToken.None);

        Assert.That(client.LastGetRoute, Is.EqualTo(ApiRoutes.CustomToolsManifest));
        Assert.That(manifest.tools, Has.Length.EqualTo(1));
        Assert.That(manifest.tools[0].name, Is.EqualTo("greet_user"));
        Assert.That(manifest.tools[0].parameters[0].name, Is.EqualTo("name"));
    }

    [Test]
    public async ValueTask Invoke_SendsToolNameAndArgumentsJson()
    {
        var client = new FakeUnityEditorClient
        {
            PostResponse = new InvokeCustomToolResponse("Hello, Alice!")
        };
        var useCase = new CustomToolUseCase(client);

        var result = await useCase.InvokeAsync("greet_user", "{\"name\":\"Alice\"}", CancellationToken.None);

        Assert.That(result, Is.EqualTo("Hello, Alice!"));
        Assert.That(client.LastPostRoute, Is.EqualTo(ApiRoutes.CustomToolsInvoke));

        var request = (InvokeCustomToolRequest)client.LastPostRequest!;
        Assert.That(request.toolName, Is.EqualTo("greet_user"));
        Assert.That(request.argumentsJson, Is.EqualTo("{\"name\":\"Alice\"}"));
    }

    [Test]
    public async ValueTask Invoke_NormalizesEmptyArgumentsJson()
    {
        var client = new FakeUnityEditorClient
        {
            PostResponse = new InvokeCustomToolResponse("pong")
        };
        var useCase = new CustomToolUseCase(client);

        await useCase.InvokeAsync("ping_custom", string.Empty, CancellationToken.None);

        var request = (InvokeCustomToolRequest)client.LastPostRequest!;
        Assert.That(request.argumentsJson, Is.EqualTo("{}"));
    }

    private sealed class FakeUnityEditorClient : IUnityEditorClient
    {
        public object? GetResponse { get; init; }
        public object? PostResponse { get; init; }

        public string? LastGetRoute { get; private set; }
        public string? LastPostRoute { get; private set; }
        public object? LastPostRequest { get; private set; }

        public ValueTask WaitForServerAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<TRes> PostAsync<TReq, TRes>(
            string route,
            TReq? request = null,
            CancellationToken cancellationToken = default) where TReq : class
        {
            LastPostRoute = route;
            LastPostRequest = request;
            return ValueTask.FromResult((TRes)PostResponse!);
        }

        public ValueTask<TRes> GetAsync<TReq, TRes>(
            string route,
            TReq? request = null,
            CancellationToken cancellationToken = default) where TReq : class
        {
            LastGetRoute = route;
            return ValueTask.FromResult((TRes)GetResponse!);
        }
    }
}
