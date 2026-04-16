using System.Text.Json;
using UniCortex.Core.Domains;
using ConsoleAppFramework;
using UniCortex.Core.UseCases;

namespace UniCortex.Cli.Commands;

public class SceneViewCommands(SceneViewUseCase sceneViewUseCase)
{
    /// <summary>Switch focus to the Scene View window.</summary>
    [Command("focus")]
    public async Task Focus(CancellationToken cancellationToken = default)
    {
        var message = await sceneViewUseCase.FocusAsync(cancellationToken);
        Console.WriteLine(message);
    }
}

public class SceneViewCameraCommands(SceneViewUseCase sceneViewUseCase)
{
    /// <summary>Get the current Scene View camera pose as JSON.</summary>
    [Command("get")]
    public async Task Get(CancellationToken cancellationToken = default)
    {
        var response = await sceneViewUseCase.GetCameraResponseAsync(cancellationToken);
        var json = JsonSerializer.Serialize(response, JsonOptions.Default);
        Console.WriteLine(json);
    }

    /// <summary>Set the Scene View camera pose using world position and rotation quaternion.</summary>
    /// <param name="positionX">Scene View camera world position X.</param>
    /// <param name="positionY">Scene View camera world position Y.</param>
    /// <param name="positionZ">Scene View camera world position Z.</param>
    /// <param name="rotationX">Scene View camera rotation quaternion X.</param>
    /// <param name="rotationY">Scene View camera rotation quaternion Y.</param>
    /// <param name="rotationZ">Scene View camera rotation quaternion Z.</param>
    /// <param name="rotationW">Scene View camera rotation quaternion W.</param>
    /// <param name="size">Optional Scene View size (zoom). Must be greater than 0 when provided.</param>
    /// <param name="orthographic">Optional orthographic toggle. true for orthographic, false for perspective.</param>
    [Command("set")]
    public async Task SetCamera(
        [Argument] float positionX,
        [Argument] float positionY,
        [Argument] float positionZ,
        [Argument] float rotationX,
        [Argument] float rotationY,
        [Argument] float rotationZ,
        [Argument] float rotationW,
        float? size = null,
        bool? orthographic = null,
        CancellationToken cancellationToken = default)
    {
        var message = await sceneViewUseCase.SetCameraAsync(
            positionX, positionY, positionZ,
            rotationX, rotationY, rotationZ, rotationW,
            size, orthographic,
            cancellationToken);
        Console.WriteLine(message);
    }
}
