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
        // Upper bound for letting the current request write its response (e.g. 408 after cancellation)
        // before the listener is closed. Keeps a stuck handler from delaying the domain reload for long.
        private static readonly TimeSpan s_currentRequestHandlingTimeout = TimeSpan.FromSeconds(1);

        private readonly IRequestRouter _router;
        private readonly int _port;
        private HttpListener _listener;
        private CancellationTokenSource _cts;

        // Completes when the handling of the current request (including writing its response) ends.
        // The listen loop handles one request at a time, so there is at most one such task.
        // Written by the listen loop and read by Stop() on the main thread, so it is guarded by _gate.
        private readonly object _gate = new();
        private Task _currentRequestHandling = Task.CompletedTask;

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
            // Start on a thread-pool thread so that async continuations never post
            // back to UnitySynchronizationContext, which stops being pumped during
            // Play Mode + Pause. All handlers that need Unity main-thread APIs
            // dispatch via MainThreadDispatcher.
            Task.Run(() => ListenLoopAsync(_cts.Token)).ContinueWith(
                t => Debug.LogError($"[UniCortex] Listen loop failed: {t.Exception}"),
                TaskContinuationOptions.OnlyOnFaulted);

            Debug.Log($"[UniCortex] Server started on http://localhost:{_port}/");
        }

        public void Stop()
        {
            if (_listener == null)
            {
                return;
            }

            _cts?.Cancel();

            // Closing the listener ends open responses as they are (an empty 200 or a dropped connection).
            // Wait for the cancelled request to finish writing its error response first.
            // Handlers resume on thread-pool threads, so they can finish while the main thread waits here;
            // a handler that still needs the main thread just runs into the timeout.
            WaitForCurrentRequestHandling();

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
                    var httpContext = await _listener.GetContextAsync();

                    // Publish the handling before the handler starts, so that Stop() waits for it
                    // even if it is called while the handler is still running synchronously.
                    var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    lock (_gate)
                    {
                        _currentRequestHandling = completion.Task;
                    }

                    try
                    {
                        await HandleContextAsync(httpContext, token);
                    }
                    finally
                    {
                        completion.TrySetResult(true);
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

        private void WaitForCurrentRequestHandling()
        {
            Task handling;
            lock (_gate)
            {
                handling = _currentRequestHandling;
            }

            if (!handling.Wait(s_currentRequestHandlingTimeout))
            {
                Debug.LogWarning("[UniCortex] Timed out waiting for the current request to respond before stopping.");
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
