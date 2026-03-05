using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class RequestRouter : IRequestRouter
    {
        private readonly
            Dictionary<(HttpMethodType method, string path), Func<IRequestContext, CancellationToken, Task>>
            _handlers = new();

        private readonly HashSet<string> _knownPaths = new();

        public void Register(HttpMethodType method, string path,
            Func<IRequestContext, CancellationToken, Task> handler)
        {
            var normalized = NormalizePath(path);
            _handlers[(method, normalized)] = handler;
            _knownPaths.Add(normalized);
        }

        public async Task HandleRequestAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var rawMethod = context.HttpMethod;
            var path = NormalizePath(context.Path);

            if (!Enum.TryParse<HttpMethodType>(rawMethod, ignoreCase: true, out var method))
            {
                await context.WriteResponseAsync(HttpStatusCodes.MethodNotAllowed,
                    JsonUtility.ToJson(new ErrorResponse("Method not allowed")));
                return;
            }

            try
            {
                if (_handlers.TryGetValue((method, path), out var handler))
                {
                    await handler(context, cancellationToken);
                }
                else if (_knownPaths.Contains(path))
                {
                    await context.WriteResponseAsync(HttpStatusCodes.MethodNotAllowed,
                        JsonUtility.ToJson(new ErrorResponse("Method not allowed")));
                }
                else
                {
                    await context.WriteResponseAsync(HttpStatusCodes.NotFound,
                        JsonUtility.ToJson(new ErrorResponse("Not found")));
                }
            }
            catch (ArgumentException ex)
            {
                Debug.LogWarning($"[UniCortex] {rawMethod} {path} invalid request: {ex.Message}");
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse("Invalid request body.")));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UniCortex] {rawMethod} {path} failed: {ex}");
                await context.WriteResponseAsync(HttpStatusCodes.InternalServerError,
                    JsonUtility.ToJson(new ErrorResponse("Internal server error")));
            }
        }

        private static string NormalizePath(string path)
        {
            var trimmed = path.TrimEnd('/');
            return string.IsNullOrEmpty(trimmed) ? "/" : trimmed;
        }
    }
}
