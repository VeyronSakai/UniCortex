using Microsoft.Extensions.Logging;

namespace UniCortex.Core.Infrastructures;

public class HttpRequestHandler(ILogger<HttpRequestHandler> logger) : DelegatingHandler
{
    private static readonly TimeSpan s_maxWait = TimeSpan.FromHours(1);
    private const int MaxServerErrorRetries = 5;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var logged = false;
        var serverErrorRetries = 0;

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

                // Retry transient server errors (5xx) a limited number of times.
                // The Unity Editor may return 500 when temporarily busy (e.g., running tests).
                if ((int)response.StatusCode >= 500 && serverErrorRetries < MaxServerErrorRetries)
                {
                    serverErrorRetries++;
                    logger.LogInformation(
                        "Unity Editor returned {StatusCode}, retrying ({Attempt}/{Max})...",
                        (int)response.StatusCode, serverErrorRetries, MaxServerErrorRetries);
                    response.Dispose();
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }

                if (logged)
                {
                    logger.LogInformation("Unity Editor is ready.");
                }

                return response;
            }
            catch (HttpRequestException) when (DateTime.UtcNow - startTime < s_maxWait)
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
}
