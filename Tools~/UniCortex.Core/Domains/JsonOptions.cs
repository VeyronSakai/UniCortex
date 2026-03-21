using System.Text.Json;

namespace UniCortex.Core.Domains;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new() { IncludeFields = true };
}
