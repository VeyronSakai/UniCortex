using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class CustomToolUseCase(IUnityEditorClient client)
{
    public async ValueTask<CustomToolListResponse> ListAsync(CancellationToken cancellationToken)
    {
        return await client.GetAsync<CustomToolListRequest, CustomToolListResponse>(
            ApiRoutes.CustomToolList, cancellationToken: cancellationToken);
    }

    public async ValueTask<string> ExecuteAsync(string name, string? argumentsJson,
        CancellationToken cancellationToken)
    {
        var request = new CustomToolExecuteRequest { name = name, arguments = argumentsJson ?? "" };
        var response = await client.PostAsync<CustomToolExecuteRequest, CustomToolExecuteResponse>(
            ApiRoutes.CustomToolExecute, request, cancellationToken);
        return response.result;
    }
}
