using System.Text;
using Microsoft.Extensions.Logging;
using UniCortex.Core.Domains;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Domains.Models;

namespace UniCortex.Core.Infrastructures;

/// <summary>
/// Fails a request that waits for the Unity Editor while a modal dialog of the Editor is open.
/// The Editor runs requests on its main thread, which does not run until the dialog is closed, so the request would
/// otherwise wait until the client times out. The dialogs are read with the OS window APIs while the request waits.
/// </summary>
public class EditorDialogWatchHandler(
    ILogger<EditorDialogWatchHandler> logger,
    IUnityEditorProcessProvider processProvider,
    IEditorDialogAccessor dialogAccessor) : DelegatingHandler
{
    // Most requests finish within this time, so the dialogs are not read for them.
    internal TimeSpan FirstCheckDelay { get; init; } = TimeSpan.FromSeconds(5);
    internal TimeSpan CheckInterval { get; init; } = TimeSpan.FromSeconds(2);

    // Without the Accessibility permission on macOS, the dialogs cannot be read. A request whose current attempt the
    // Editor has not answered for this time then fails, instead of waiting until the client times out (10 minutes).
    // Requests whose attempts end (e.g. GET /tests/result while tests run, or refused during a domain reload) keep
    // waiting, but a long operation that blocks the main thread (e.g. a large import) is not told apart from a dialog.
    public const int NoAnswerTimeoutSeconds = 60;

    internal TimeSpan NoAnswerTimeout { get; init; } = TimeSpan.FromSeconds(NoAnswerTimeoutSeconds);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var attemptClock = new RequestAttemptClock();
        request.Options.Set(RequestAttemptClock.Key, attemptClock);
        using var sendCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var sendTask = base.SendAsync(request, sendCts.Token);
        var delay = FirstCheckDelay;
        var failureLogged = false;

        while (true)
        {
            var completed = await Task.WhenAny(sendTask, Task.Delay(delay, cancellationToken));
            if (completed == sendTask)
            {
                return await sendTask;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // The OS window APIs can block for a while (e.g. the Accessibility API waits for the Editor to answer).
            var (dialogs, error) = await Task.Run(ReadDialogs, cancellationToken);
            if (error != null && !failureLogged)
            {
                // Without the dialogs (e.g. on Linux), the request keeps waiting as before.
                logger.LogWarning("Could not check the Unity Editor's modal dialogs: {Message}", error.Message);
                failureLogged = true;
            }

            if (sendTask.IsCompleted)
            {
                return await sendTask;
            }

            if (dialogs.Count > 0)
            {
                await AbandonAsync(sendCts, sendTask);
                throw new InvalidOperationException(CreateBlockedByDialogMessage(dialogs));
            }

            if (error is AccessibilityPermissionException && attemptClock.CurrentAttemptDuration >= NoAnswerTimeout)
            {
                await AbandonAsync(sendCts, sendTask);
                throw new InvalidOperationException(CreateNoAnswerWithoutPermissionMessage(NoAnswerTimeout));
            }

            delay = CheckInterval;
        }
    }

    // Cancels the request and stops waiting for it. The request is not cancelled in the Editor.
    private static async Task AbandonAsync(CancellationTokenSource sendCts, Task<HttpResponseMessage> sendTask)
    {
        await sendCts.CancelAsync();
        _ = sendTask.ContinueWith(t => t.Exception, TaskContinuationOptions.OnlyOnFaulted);
    }

    private (IReadOnlyList<EditorDialog> Dialogs, Exception? Error) ReadDialogs()
    {
        try
        {
            return (dialogAccessor.GetDialogs(processProvider.GetProcessId()), null);
        }
        catch (Exception ex)
        {
            return ([], ex);
        }
    }

    private static string CreateBlockedByDialogMessage(IReadOnlyList<EditorDialog> dialogs)
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            "The Unity Editor is blocked by a modal dialog, so it cannot respond until the dialog is closed.");
        foreach (var dialog in dialogs)
        {
            builder.AppendLine();
            builder.AppendLine($"Title: {dialog.title}");
            if (dialog.message.Length > 0)
            {
                builder.AppendLine($"Message: {dialog.message}");
            }

            builder.AppendLine(dialog.buttons.Count > 0
                ? $"Buttons: {string.Join(", ", dialog.buttons.Select(b => $"\"{b}\""))}"
                : "Buttons: none could be read");
        }

        builder.AppendLine();
        builder.Append(
            "The request was already sent to the Editor: it may have opened the dialog, or it may still run after " +
            "the dialog is closed. Close the dialog with click_editor_dialog_button (CLI: editor dialog click " +
            "<button>), or ask the user to close it, then check the result before retrying the request.");
        if (dialogs.Any(dialog => dialog.buttons.Count == 0))
        {
            builder.Append(' ').Append(EditorDialogButtonLabel.UnreadButtonsHint);
        }

        return builder.ToString();
    }

    private static string CreateNoAnswerWithoutPermissionMessage(TimeSpan timeout)
    {
        return
            $"The Unity Editor has not responded for {timeout.TotalSeconds:0} seconds. A modal dialog may be open " +
            "in the Editor, but UniCortex cannot read the Editor's dialogs because the Accessibility permission is " +
            "not granted. Ask the user to close the dialog in the Editor if one is open. To let " +
            "get_editor_dialogs and click_editor_dialog_button read and close the dialogs, the user has to grant " +
            "the permission: " + AccessibilityPermissionException.HowToGrant + " The request was already sent to " +
            "the Editor: it may still be running (e.g. a long import), or it may run after the dialog is closed, so " +
            "check the result before retrying the request.";
    }
}
