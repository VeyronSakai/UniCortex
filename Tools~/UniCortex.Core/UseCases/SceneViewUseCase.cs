using UniCortex.Core.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Core.UseCases;

public class SceneViewUseCase(IUnityEditorClient client)
{
    public async ValueTask<string> FocusAsync(CancellationToken cancellationToken)
    {
        await client.PostAsync<FocusSceneViewRequest, FocusSceneViewResponse>(
            ApiRoutes.FocusSceneView, cancellationToken: cancellationToken);
        return "Scene View focused successfully.";
    }

    public async ValueTask<string> SetCameraAsync(
        float positionX,
        float positionY,
        float positionZ,
        float rotationX,
        float rotationY,
        float rotationZ,
        float rotationW,
        float? size,
        bool? orthographic,
        CancellationToken cancellationToken)
    {
        await client.PostAsync<SetSceneViewCameraRequest, SetSceneViewCameraResponse>(
            ApiRoutes.SceneViewCamera,
            new SetSceneViewCameraRequest
            {
                position = new Vector3Data { x = positionX, y = positionY, z = positionZ },
                rotation = new QuaternionData
                {
                    x = rotationX,
                    y = rotationY,
                    z = rotationZ,
                    w = rotationW
                },
                size = size,
                orthographic = orthographic
            },
            cancellationToken);
        return "Scene View camera set successfully.";
    }
}
