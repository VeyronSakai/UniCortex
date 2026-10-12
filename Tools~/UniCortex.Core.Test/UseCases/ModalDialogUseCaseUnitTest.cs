using NUnit.Framework;
using UniCortex.Core.Domains.Interfaces;
using UniCortex.Core.Domains.Models;
using UniCortex.Core.UseCases;

namespace UniCortex.Core.Test.UseCases;

[TestFixture]
public class ModalDialogUseCaseUnitTest
{
    private const int ProcessId = 1234;

    [Test]
    public async ValueTask GetDialogsAsync_ReturnsDialogsAsJson()
    {
        // Arrange
        var accessor = new FakeModalDialogAccessor();
        accessor.Dialogs.Add(new ModalDialog
        {
            title = "Saving failed",
            message = "Auto Save has been temporarily disabled.",
            buttons = ["OK"]
        });
        var useCase = new ModalDialogUseCase(new StubProcessProvider(), accessor);

        // Act
        var json = await useCase.GetDialogsAsync(CancellationToken.None);

        // Assert
        Assert.That(json, Is.EqualTo(
            "{\"dialogs\":[{\"title\":\"Saving failed\"," +
            "\"message\":\"Auto Save has been temporarily disabled.\",\"buttons\":[\"OK\"]}]}"));
        Assert.That(accessor.ProcessIds, Is.EqualTo(new[] { ProcessId }));
    }

    [Test]
    public async ValueTask GetDialogsAsync_DoesNotEscapeNonAsciiCharacters()
    {
        // Arrange
        var accessor = new FakeModalDialogAccessor();
        accessor.Dialogs.Add(new ModalDialog { title = "保存に失敗しました", buttons = ["Don't Save"] });
        var useCase = new ModalDialogUseCase(new StubProcessProvider(), accessor);

        // Act
        var json = await useCase.GetDialogsAsync(CancellationToken.None);

        // Assert
        Assert.That(json, Does.Contain("保存に失敗しました"));
        Assert.That(json, Does.Contain("Don't Save"));
    }

    [Test]
    public async ValueTask GetDialogsAsync_ReturnsEmptyList_WhenNoDialogIsOpen()
    {
        // Arrange
        var useCase = new ModalDialogUseCase(new StubProcessProvider(), new FakeModalDialogAccessor());

        // Act
        var json = await useCase.GetDialogsAsync(CancellationToken.None);

        // Assert
        Assert.That(json, Is.EqualTo("{\"dialogs\":[]}"));
    }

    [Test]
    public async ValueTask ClickButtonAsync_ClicksButtonOfEditorProcess()
    {
        // Arrange
        var accessor = new FakeModalDialogAccessor();
        accessor.Dialogs.Add(new ModalDialog { title = "Saving failed", buttons = ["OK"] });
        var useCase = new ModalDialogUseCase(new StubProcessProvider(), accessor);

        // Act
        var message = await useCase.ClickButtonAsync("ok", CancellationToken.None);

        // Assert
        Assert.That(message, Is.EqualTo("Clicked \"OK\" in the dialog \"Saving failed\"."));
        Assert.That(accessor.ClickedButtons, Is.EqualTo(new[] { "ok" }));
        Assert.That(accessor.ProcessIds, Is.EqualTo(new[] { ProcessId }));
    }

    [Test]
    public void ClickButtonAsync_Throws_WhenNoDialogIsOpen()
    {
        // Arrange
        var useCase = new ModalDialogUseCase(new StubProcessProvider(), new FakeModalDialogAccessor());

        // Act
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await useCase.ClickButtonAsync("OK", CancellationToken.None));

        // Assert
        Assert.That(ex!.Message, Does.Contain("No modal dialog is open"));
    }

    [Test]
    public void ClickButtonAsync_ThrowsWithAvailableButtons_WhenButtonIsNotFound()
    {
        // Arrange
        var accessor = new FakeModalDialogAccessor();
        accessor.Dialogs.Add(new ModalDialog { title = "Scene(s) Have Been Modified", buttons = ["Save", "Cancel"] });
        var useCase = new ModalDialogUseCase(new StubProcessProvider(), accessor);

        // Act
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await useCase.ClickButtonAsync("OK", CancellationToken.None));

        // Assert
        Assert.That(ex!.Message, Does.Contain("No button labeled \"OK\""));
        Assert.That(ex.Message, Does.Contain("\"Save\", \"Cancel\""));
    }

    private sealed class StubProcessProvider : IUnityEditorProcessProvider
    {
        public int GetProcessId()
        {
            return ProcessId;
        }
    }

    private sealed class FakeModalDialogAccessor : IModalDialogAccessor
    {
        public List<ModalDialog> Dialogs { get; } = [];
        public List<int> ProcessIds { get; } = [];
        public List<string> ClickedButtons { get; } = [];

        public IReadOnlyList<ModalDialog> GetDialogs(int processId)
        {
            ProcessIds.Add(processId);
            return Dialogs;
        }

        public ModalDialog? ClickButton(int processId, string buttonLabel)
        {
            ProcessIds.Add(processId);
            var dialog = Dialogs.FirstOrDefault(d =>
                d.buttons.Any(b => string.Equals(b, buttonLabel, StringComparison.OrdinalIgnoreCase)));
            if (dialog != null)
            {
                ClickedButtons.Add(buttonLabel);
            }

            return dialog;
        }
    }
}
