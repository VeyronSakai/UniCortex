using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class HttpListenerServer : IHttpServer
    {
        private readonly IRequestRouter _router;
        private readonly int _port;
        private readonly EditorStateCache _stateCache;
        private readonly SynchronizationContext _unitySyncContext;
        private HttpListener _listener;
        private CancellationTokenSource _cts;

        public HttpListenerServer(IRequestRouter router, int port,
            EditorStateCache stateCache = null)
        {
            _router = router;
            _port = port;
            _stateCache = stateCache;
            // Capture UnitySynchronizationContext so we can post non-direct
            // handlers back to the main thread after ConfigureAwait(false).
            _unitySyncContext = SynchronizationContext.Current;
        }

        public void Start()
        {
            if (_listener != null)
            {
                return;
            }

            _listener = new HttpListener();

            try
            {
                _listener.Prefixes.Add($"http://localhost:{_port}/");
                _listener.Start();
            }
            catch
            {
                Debug.LogError($"[UniCortex] Failed to start listening on port {_port}");

                try
                {
                    _listener.Close();
                }
                catch
                {
                    Debug.LogError($"[UniCortex] Failed to close listener after failed start on port {_port}");
                }
                finally
                {
                    _listener = null;
                }

                throw;
            }

            _cts = new CancellationTokenSource();
            _ = ListenLoopAsync(_cts.Token);

            Debug.Log($"[UniCortex] Server started on http://localhost:{_port}/");
        }

        public void Stop()
        {
            if (_listener == null)
            {
                return;
            }

            _cts?.Cancel();

            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch
            {
                Debug.LogError($"[UniCortex] Failed to stop listener on port {_port}");
            }

            _listener = null;
            _cts = null;
        }

        private async Task ListenLoopAsync(CancellationToken token)
        {
            while (_listener is { IsListening: true } && !token.IsCancellationRequested)
            {
                try
                {
                    // ConfigureAwait(false) so the continuation runs on a threadpool thread
                    // instead of posting back to UnitySynchronizationContext (which stops
                    // being pumped during Play Mode + Pause).
                    var httpContext = await _listener.GetContextAsync().ConfigureAwait(false);

                    var path = httpContext.Request.Url?.AbsolutePath?.TrimEnd('/');
                    if (IsPauseResistantEndpoint(path, httpContext.Request.HttpMethod))
                    {
                        // Handle on threadpool — these endpoints avoid Unity APIs
                        // and must respond even during Play Mode + Pause.
                        _ = Task.Run(() => HandleDirectAsync(httpContext));
                    }
                    else if (_unitySyncContext != null)
                    {
                        // Post to main thread via UnitySynchronizationContext.
                        // Handlers that call Unity APIs (JsonUtility, SessionState, etc.)
                        // need to run on the main thread.
                        var ctx = httpContext;
                        var t = token;
                        _unitySyncContext.Post(_ => { _ = HandleContextAsync(ctx, t); }, null);
                    }
                    else
                    {
                        _ = HandleContextAsync(httpContext, token);
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (HttpListenerException)
                {
                    break;
                }
            }
        }

        private static bool IsPauseResistantEndpoint(string path, string method)
        {
            if (path == "/editor/status") return true;
            if (path == "/editor/pause" && string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
                return true;
            if (path == "/editor/unpause" && string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private async Task HandleDirectAsync(HttpListenerContext httpContext)
        {
            try
            {
                var path = httpContext.Request.Url?.AbsolutePath?.TrimEnd('/');

                if (path == "/editor/status" && _stateCache != null)
                {
                    await WriteDirectResponse(httpContext, 200,
                        "{\"isPlaying\":" + (_stateCache.IsPlaying ? "true" : "false") +
                        ",\"isPaused\":" + (_stateCache.IsPaused ? "true" : "false") + "}")
                        .ConfigureAwait(false);
                    return;
                }

                if (path == "/editor/unpause" && _stateCache != null)
                {
                    _stateCache.RequestUnpause();
                    await WriteDirectResponse(httpContext, 200, "{\"success\":true}")
                        .ConfigureAwait(false);
                    return;
                }

                if (path == "/editor/pause" && _stateCache != null)
                {
                    _stateCache.RequestPause();
                    await WriteDirectResponse(httpContext, 200, "{\"success\":true}")
                        .ConfigureAwait(false);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniCortex] Direct handler failed: {e}");
            }
        }

        private async Task HandleContextAsync(HttpListenerContext httpContext, CancellationToken token)
        {
            var context = new HttpListenerRequestContext(httpContext);
            try
            {
                await _router.HandleRequestAsync(context, token);
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniCortex] Request handling failed: {e}");

                try
                {
                    await context.WriteResponseAsync(Domains.Models.HttpStatusCodes.InternalServerError,
                        UnityEngine.JsonUtility.ToJson(new Domains.Models.ErrorResponse("Internal server error")));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[UniCortex] Failed to write error response: {ex}");
                }
            }
        }

        private static async Task WriteDirectResponse(HttpListenerContext httpContext, int statusCode, string json)
        {
            var response = httpContext.Response;
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            response.KeepAlive = false;
            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            try
            {
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            }
            finally
            {
                response.OutputStream.Close();
            }
        }
    }
}
