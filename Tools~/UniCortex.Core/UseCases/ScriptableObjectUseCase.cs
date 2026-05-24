using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class ScriptableObjectUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> CreateAsync(string typeName, string assetPath,
        CancellationToken cancellationToken)
    {
        var request = new CreateScriptableObjectRequest { typeName = typeName, assetPath = assetPath };
        var response = await client.PostAsync<CreateScriptableObjectRequest, CreateScriptableObjectResponse>(
            ApiRoutes.ScriptableObjectCreate, request, cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> GetPropertiesAsync(string assetPath,
        CancellationToken cancellationToken = default)
    {
        var request = new GetScriptableObjectPropertiesRequest { assetPath = assetPath };
        var response =
            await client
                .GetAsync<GetScriptableObjectPropertiesRequest, GetScriptableObjectPropertiesResponse>(
                    ApiRoutes.ScriptableObjectProperties, request, cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> SetPropertyAsync(string assetPath, string propertyPath, string value,
        CancellationToken cancellationToken = default)
    {
        var request = new SetScriptableObjectPropertyRequest
        {
            assetPath = assetPath,
            propertyPath = propertyPath,
            value = value
        };
        await client.PostAsync<SetScriptableObjectPropertyRequest, SetScriptableObjectPropertyResponse>(
            ApiRoutes.ScriptableObjectProperty, request, cancellationToken);
        return $"Property '{propertyPath}' set to '{value}' successfully.";
    }
}
