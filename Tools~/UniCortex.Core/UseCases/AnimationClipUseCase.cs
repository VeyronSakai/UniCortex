using System.Text.Json;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class AnimationClipUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> CreateAsync(string assetPath, bool loop, float frameRate,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateAnimationClipRequest { assetPath = assetPath, loop = loop, frameRate = frameRate };
        var response = await client.PostAsync<CreateAnimationClipRequest, CreateAnimationClipResponse>(
            ApiRoutes.AnimationClipCreate, request, cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> GetCurvesAsync(string assetPath, CancellationToken cancellationToken = default)
    {
        var request = new GetAnimationCurvesRequest { assetPath = assetPath };
        var response = await client.GetAsync<GetAnimationCurvesRequest, GetAnimationCurvesResponse>(
            ApiRoutes.AnimationClipCurves, request, cancellationToken);
        return JsonSerializer.Serialize(response, JsonOptions.Default);
    }

    public async ValueTask<string> SetCurveAsync(string assetPath, string path, string componentType,
        string assemblyName, string propertyName, List<AnimationCurveKeyInput> keys,
        CancellationToken cancellationToken = default)
    {
        var request = new SetAnimationCurveRequest
        {
            assetPath = assetPath,
            path = path,
            componentType = componentType,
            assemblyName = assemblyName,
            propertyName = propertyName,
            keys = keys
        };
        await client.PostAsync<SetAnimationCurveRequest, SetAnimationCurveResponse>(
            ApiRoutes.AnimationClipSetCurve, request, cancellationToken);
        return $"Curve '{path}:{componentType}.{propertyName}' set with {keys.Count} key(s).";
    }

    public async ValueTask<string> RemoveCurveAsync(string assetPath, string path, string componentType,
        string assemblyName, string propertyName, CancellationToken cancellationToken = default)
    {
        var request = new RemoveAnimationCurveRequest
        {
            assetPath = assetPath,
            path = path,
            componentType = componentType,
            assemblyName = assemblyName,
            propertyName = propertyName
        };
        await client.PostAsync<RemoveAnimationCurveRequest, RemoveAnimationCurveResponse>(
            ApiRoutes.AnimationClipRemoveCurve, request, cancellationToken);
        return $"Curve '{path}:{componentType}.{propertyName}' removed.";
    }

    /// <summary>
    /// Parses a JSON array of keys (e.g. [{"time":0,"value":0,"tangentMode":"Linear"}]).
    /// </summary>
    public static List<AnimationCurveKeyInput> ParseKeys(string keysJson)
    {
        var options = new JsonSerializerOptions(JsonOptions.Default) { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<List<AnimationCurveKeyInput>>(keysJson, options)
               ?? throw new ArgumentException("keys must be a JSON array.");
    }
}
