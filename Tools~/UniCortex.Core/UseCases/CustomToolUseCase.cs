using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class CustomToolUseCase(IUnityEditorClient client)
{
    public async ValueTask<GetCustomToolsManifestResponse> GetManifestAsync(CancellationToken cancellationToken)
    {
        return await client.GetAsync<GetCustomToolsManifestRequest, GetCustomToolsManifestResponse>(
            ApiRoutes.CustomToolsManifest,
            cancellationToken: cancellationToken);
    }

    public async ValueTask<string> InvokeAsync(
        string toolName,
        string argumentsJson,
        CancellationToken cancellationToken)
    {
        var request = new InvokeCustomToolRequest
        {
            toolName = toolName,
            argumentsJson = string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson
        };

        var response = await client.PostAsync<InvokeCustomToolRequest, InvokeCustomToolResponse>(
            ApiRoutes.CustomToolsInvoke,
            request,
            cancellationToken);
        return response.content;
    }
}
