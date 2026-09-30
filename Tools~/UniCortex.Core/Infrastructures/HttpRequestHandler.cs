using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace UniCortex.Core.Infrastructures;

public class HttpRequestHandler(ILogger<HttpRequestHandler> logger) : DelegatingHandler
{
    private static readonly TimeSpan s_maxWait = TimeSpan.FromHours(1);

    /// <summary>
    /// Set to false for requests that must not be sent twice (e.g. starting a test run).
    /// Such requests are retried only when the connection is refused, i.e. the server never received them.
    /// </summary>
    public static readonly HttpRequestOptionsKey<bool> ResendAfterDisconnectKey = new("UniCortex.ResendAfterDisconnect");

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var logged = false;
        var resendAfterDisconnect = !request.Options.TryGetValue(ResendAfterDisconnectKey, out var resend) || resend;

        while (true)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                // If the domain is reloading, a response with Content-Length 0 may be returned, which will also be considered a failure and will be retried.
                if (response.Content.Headers.ContentLength is null or 0)
                {
                    response.Dispose();
                    throw new HttpRequestException();
                }

                // The server answers 503 when it is stopped (e.g. by a domain reload) while handling the request.
                // Like an empty response, this means the server may have received the request, so it is retried
                // only when resending is allowed. Otherwise the 503 is returned to the caller as is.
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable && resendAfterDisconnect)
                {
                    response.Dispose();
                    throw new HttpRequestException();
                }

                if (logged)
                {
                    logger.LogInformation("Unity Editor is ready.");
                }

                return response;
            }
            catch (HttpRequestException ex) when (DateTime.UtcNow - startTime < s_maxWait &&
                                                  (resendAfterDisconnect || IsConnectionRefused(ex)))
            {
                if (!logged)
                {
                    logger.LogInformation(
                        "Unity Editor is not responding. Waiting for domain reload to complete...");
                    logged = true;
                }

                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    // A refused connection means the server is not listening (e.g. it is stopped for a domain reload),
    // so the request never reached it and resending cannot run it twice. Requests that must not be resent
    // still have to be retried in this case: a caller that gave up and polled for the outcome instead
    // (e.g. GET /tests/result after POST /tests/run) would wait for a run that never started and get
    // the result of a previous run.
    private static bool IsConnectionRefused(HttpRequestException exception)
    {
        return exception.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused };
    }
}
