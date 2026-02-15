using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EditorBridge.Editor.Server
{
    internal class RequestRouter
    {
        readonly Dictionary<(string method, string path), Func<HttpListenerContext, Task>> _handlers =
            new Dictionary<(string method, string path), Func<HttpListenerContext, Task>>();

        readonly HashSet<string> _knownPaths = new HashSet<string>();

        public void Register(string method, string path, Func<HttpListenerContext, Task> handler)
        {
            var normalized = NormalizePath(path);
            _handlers[(method.ToUpperInvariant(), normalized)] = handler;
            _knownPaths.Add(normalized);
        }

        public async Task HandleRequest(HttpListenerContext context)
        {
            var method = context.Request.HttpMethod.ToUpperInvariant();
            var path = NormalizePath(context.Request.Url.AbsolutePath);

            try
            {
                if (_handlers.TryGetValue((method, path), out var handler))
                {
                    await handler(context);
                }
                else if (_knownPaths.Contains(path))
                {
                    WriteResponse(context, 405, JsonHelper.Error("Method not allowed"));
                }
                else
                {
                    WriteResponse(context, 404, JsonHelper.Error("Not found"));
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[EditorBridge] {method} {path} failed: {ex}");
                WriteResponse(context, 500, JsonHelper.Error("Internal server error"));
            }
        }

        public static void WriteResponse(HttpListenerContext context, int statusCode, string json)
        {
            var response = context.Response;
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            var buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            try
            {
                response.OutputStream.Close();
            }
            catch
            {
                // client may have already disconnected
            }
        }

        static string NormalizePath(string path)
        {
            var trimmed = path.TrimEnd('/');
            return string.IsNullOrEmpty(trimmed) ? "/" : trimmed;
        }
    }
}
