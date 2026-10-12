using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Logging;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Domains.Models;

namespace UniCortex.Core.Infrastructures;

public class HttpRequestHandler(
    ILogger<HttpRequestHandler> logger,
    IUnityEditorProcessProvider processProvider,
    IEditorDialogAccessor dialogAccessor) : DelegatingHandler
{
    private static readonly TimeSpan s_maxWait = TimeSpan.FromHours(1);

    // Every request is resent until the server answers, so that requests survive domain reloads.
    // - Connection refused: the server is not listening, so it never received the request.
    // - 503: the server was stopped before running the request. Requests run on the main thread, and so does
    //   stopping the server: either the request ran first and its response is written before the server closes,
    //   or the stop came first and the queued request was dropped without running.
    // - A response not written by a request handler (no JSON content, e.g. an empty 200 or a 400 that the
    //   listener writes itself while it is closing): the request never reached a handler, so it has not run.
    //   A request sent right after a domain-reload response, on the same kept-alive connection, can get one.
    // - Dropped connection or empty response: it is unknown whether the server ran the request. This hardly
    //   happens, since the server writes the response of the current request before it stops. It remains when
    //   the Editor crashes, or when a request still queued in the listener is dropped without running; resending
    //   is right in both cases. Only a response not written within the server's stop timeout can make a request
    //   run twice.
    // - 504: the Editor's main thread did not run the request in time, so it has not run. If a modal dialog of the
    //   Editor is open, the main thread does not run until the dialog is closed, so the request fails at once with
    //   the dialog's contents. Otherwise the Editor is busy (e.g. importing assets), and the request is resent.
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var logged = false;
        var dialogCheckFailureLogged = false;

        while (true)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                // Every response written by a request handler is JSON. While the server is stopping (e.g. for a domain
                // reload), the listener may answer by itself with an empty or non-JSON response. The server also
                // answers 503 when it is stopped before running the request. All of them are retried.
                if (response.Content.Headers.ContentLength is null or 0 ||
                    response.Content.Headers.ContentType?.MediaType != MediaTypeNames.Application.Json ||
                    response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    response.Dispose();
                    throw new HttpRequestException();
                }

                if (response.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    response.Dispose();
                    var dialogs = GetOpenDialogs(ref dialogCheckFailureLogged);
                    if (dialogs.Count > 0)
                    {
                        throw new InvalidOperationException(CreateBlockedByDialogMessage(dialogs));
                    }

                    throw new HttpRequestException();
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
                        "Unity Editor is not responding (e.g. reloading the domain or busy). Waiting...");
                    logged = true;
                }

                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    private IReadOnlyList<EditorDialog> GetOpenDialogs(ref bool failureLogged)
    {
        try
        {
            return dialogAccessor.GetDialogs(processProvider.GetProcessId());
        }
        catch (Exception ex)
        {
            // Without the dialogs (e.g. on Linux, or without the Accessibility permission on macOS), the request is
            // resent as before, because a busy Editor cannot be told apart from a blocked one.
            if (!failureLogged)
            {
                logger.LogWarning("Could not check the Unity Editor's modal dialogs: {Message}", ex.Message);
                failureLogged = true;
            }

            return [];
        }
    }

    private static string CreateBlockedByDialogMessage(IReadOnlyList<EditorDialog> dialogs)
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            "The Unity Editor is blocked by a modal dialog and cannot handle requests until it is closed. " +
            "The request was not run.");
        foreach (var dialog in dialogs)
        {
            builder.AppendLine();
            builder.AppendLine($"Title: {dialog.title}");
            if (dialog.message.Length > 0)
            {
                builder.AppendLine($"Message: {dialog.message}");
            }

            builder.AppendLine($"Buttons: {string.Join(", ", dialog.buttons.Select(b => $"\"{b}\""))}");
        }

        builder.AppendLine();
        builder.Append(
            "Close the dialog with click_editor_dialog_button (CLI: editor dialog click <button>), " +
            "or ask the user to close it, then retry the request.");
        return builder.ToString();
    }
}
