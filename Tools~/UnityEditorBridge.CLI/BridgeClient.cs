using System.Text.Json;

namespace UnityEditorBridge.CLI;

public static class BridgeClient
{
    private static readonly HttpClient s_httpClient = new()
    {
        BaseAddress = new Uri(
            Environment.GetEnvironmentVariable("UEB_URL") ?? "http://localhost:56780")
    };

    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true, IncludeFields = true };

    public static async Task GetAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            var response = await s_httpClient.GetAsync(path, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await Console.Error.WriteLineAsync(
                    $"Error: {(int)response.StatusCode} {response.StatusCode}{Environment.NewLine}{body}");
                Environment.Exit(1);
                return;
            }

            var json = JsonSerializer.Deserialize<JsonElement>(body);
            Console.WriteLine(JsonSerializer.Serialize(json, s_jsonOptions));
        }
        catch (HttpRequestException ex)
        {
            await Console.Error.WriteLineAsync($"Error: Could not connect to server. {ex.Message}");
            Environment.Exit(1);
        }
    }
}
