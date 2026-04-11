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
        private readonly Dictionary<(HttpMethodType method, string path),
            Func<IRequestContext, CancellationToken, Task>> _handlers = new();

        public void Register(HttpMethodType method, string path,
            Func<IRequestContext, CancellationToken, Task> handler)
        {
            var normalized = NormalizePath(path);
            var routeKey = (method, normalized);
            if (_handlers.ContainsKey(routeKey))
            {
                throw new ArgumentException($"Route already registered: {method} {normalized}");
            }

            _handlers[routeKey] = handler;
        }

        public async Task HandleRequestAsync(IRequestContext context, CancellationToken cancellationToken)
        {
            var method = context.HttpMethod;
            var path = NormalizePath(context.Path);

            try
            {
                if (_handlers.TryGetValue((method, path), out var handler))
                {
                    await handler(context, cancellationToken);
                }
                else
                {
                    await context.WriteResponseAsync(HttpStatusCodes.NotFound,
                        JsonUtility.ToJson(new ErrorResponse("Not found")));
                }
            }
            catch (ArgumentException ex)
            {
                Debug.LogWarning($"[UniCortex] {method} {path} invalid request: {ex.Message}");
                await context.WriteResponseAsync(HttpStatusCodes.BadRequest,
                    JsonUtility.ToJson(new ErrorResponse($"Invalid request: {ex.Message}")));
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[UniCortex] {method} {path} request was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UniCortex] {method} {path} failed: {ex}");
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
