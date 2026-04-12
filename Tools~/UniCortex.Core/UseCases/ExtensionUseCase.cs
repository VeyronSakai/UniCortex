using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ExtensionUseCase(IUnityEditorClient client)
{
    public async ValueTask<ExtensionListResponse> ListAsync(CancellationToken cancellationToken)
    {
        return await client.GetAsync<ExtensionListRequest, ExtensionListResponse>(
            ApiRoutes.ExtensionList, cancellationToken: cancellationToken);
    }

    public async ValueTask<string> ExecuteAsync(string name, string? argumentsJson,
        CancellationToken cancellationToken)
    {
        var request = new ExtensionExecuteRequest { name = name, arguments = argumentsJson ?? "" };
        var response = await client.PostAsync<ExtensionExecuteRequest, ExtensionExecuteResponse>(
            ApiRoutes.ExtensionExecute, request, cancellationToken);
        return response.result;
    }
}
