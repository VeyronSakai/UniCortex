using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UnityEngine;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class HttpListenerServer : IHttpServer
    {
        private readonly IRequestRouter _router;
        private readonly int _port;
        private HttpListener _listener;
        private CancellationTokenSource _cts;

        public HttpListenerServer(IRequestRouter router, int port)
        {
            _router = router;
            _port = port;
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
            var token = _cts.Token;
            var requestQueue = new SequentialRequestQueue<HttpListenerContext>();
            // Accept on a thread-pool thread so async continuations never post back
            // to UnitySynchronizationContext. A single request worker drains the
            // queue in FIFO order so Unity-side request execution stays serialized.
            Task.Run(() => requestQueue.RunAsync(HandleContextAsync, token)).ContinueWith(
                t =>
                {
                    requestQueue.Dispose();
                    if (t.Exception != null)
                    {
                        Debug.LogError($"[UniCortex] Request processor failed: {t.Exception}");
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);
            Task.Run(() => ListenLoopAsync(requestQueue, token)).ContinueWith(
                t => Debug.LogError($"[UniCortex] Listen loop failed: {t.Exception}"),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);

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

        private async Task ListenLoopAsync(SequentialRequestQueue<HttpListenerContext> requestQueue,
            CancellationToken token)
        {
            var listener = _listener;
            if (listener == null)
            {
                return;
            }

            while (listener.IsListening && !token.IsCancellationRequested)
            {
                try
                {
                    var httpContext = await listener.GetContextAsync();
                    requestQueue.Enqueue(httpContext);
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

        private async Task HandleContextAsync(HttpListenerContext httpContext, CancellationToken token)
        {
            var context = new HttpListenerRequestContext(httpContext);
            try
            {
                await _router.HandleRequestAsync(context, token);
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning(
                    $"[UniCortex] {httpContext.Request.HttpMethod} {httpContext.Request.Url.AbsolutePath} request was cancelled: {e.Message}");
                await TryWriteExceptionResponseAsync(context, e);
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniCortex] Request handling failed: {e}");
                await TryWriteExceptionResponseAsync(context, e);
            }
        }

        private static async Task TryWriteExceptionResponseAsync(IRequestContext context, Exception exception)
        {
            try
            {
                await RequestExceptionResponder.RespondAsync(context, exception);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (HttpListenerException)
            {
            }
        }
    }
}
