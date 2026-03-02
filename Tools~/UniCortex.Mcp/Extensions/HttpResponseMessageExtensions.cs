using System.Net.Http.Json;
using System.Text.Json;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Mcp.Extensions;

internal static class HttpResponseMessageExtensions
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { IncludeFields = true };

    /// <summary>
    /// Throws <see cref="HttpRequestException"/> with the error message from the Unity Editor
    /// response body when the status code indicates failure.
    /// </summary>
    internal static async Task EnsureSuccessWithErrorBodyAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string? errorMessage = null;
        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var error = JsonSerializer.Deserialize<ErrorResponse>(body, s_jsonOptions);
            errorMessage = error?.error;
        }
        catch
        {
            // The response body may be empty, non-JSON, or unreadable due to network issues.
            // Swallow the exception so the caller receives a clear HTTP error instead of
            // an unrelated deserialization/IO exception.
        }

        throw new HttpRequestException(
            string.IsNullOrEmpty(errorMessage)
                ? $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"
                : errorMessage);
    }
}
