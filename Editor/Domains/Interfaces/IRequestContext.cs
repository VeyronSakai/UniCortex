namespace EditorBridge.Editor.Domains.Interfaces
{
    internal interface IRequestContext
    {
        string HttpMethod { get; }
        string Path { get; }
        string ReadBody();
        void WriteResponse(int statusCode, string json);
    }
}
