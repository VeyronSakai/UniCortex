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
        // Upper bound for letting an in-flight request write its response (e.g. 408 after cancellation)
        // before the listener is closed. Keeps a stuck handler from delaying the domain reload for long.
        private static readonly TimeSpan s_inFlightResponseTimeout = TimeSpan.FromSeconds(1);

        private readonly IRequestRouter _router;
        private readonly int _port;
        private HttpListener _listener;
        private CancellationTokenSource _cts;

        // The listen loop handles one request at a time, so this is the only request in flight.
        private Task _inFlightRequest = Task.CompletedTask;

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
            WaitForInFlightRequest();

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

                    // Publish the request as in flight before the handler starts, so that Stop() waits for it
                    // even if it is called while the handler is still running synchronously.
                    var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    Volatile.Write(ref _inFlightRequest, completion.Task);
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

        private void WaitForInFlightRequest()
        {
            var request = Volatile.Read(ref _inFlightRequest);
            if (!request.Wait(s_inFlightResponseTimeout))
            {
                Debug.LogWarning("[UniCortex] Timed out waiting for the in-flight request to respond before stopping.");
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
