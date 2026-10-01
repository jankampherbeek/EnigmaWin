// UnsavedChangesGuardTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Threading.Tasks;
using EnigmaWin.Sources.AppShell.State;

namespace EnigmaWintest.AppShell.State;

[TestFixture]
public class UnsavedChangesGuardTests
{
    private sealed class FakeEditor : IUnsavedChangesEditor
    {
        public bool IsDirty { get; set; }
        public int SaveCount { get; private set; }
        public int RevertCount { get; private set; }

        public Task SaveAsync() { SaveCount++; IsDirty = false; return Task.CompletedTask; }
        public void Revert()    { RevertCount++; IsDirty = false; }
    }

    private static (UnsavedChangesGuard Guard, FakeEditor Editor, int[] AskCount) Make(
        UnsavedChangesChoice answer, bool isDirty = true)
    {
        var askCount = new int[1];
        var guard    = new UnsavedChangesGuard(() => { askCount[0]++; return answer; });
        var editor   = new FakeEditor { IsDirty = isDirty };
        guard.Track(editor);
        return (guard, editor, askCount);
    }

    [Test]
    public void Perform_NoUnsavedChanges_RunsActionWithoutAsking()
    {
        var (guard, _, askCount) = Make(UnsavedChangesChoice.Cancel, isDirty: false);
        var done = false;

        guard.Perform(() => done = true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(done, Is.True);
            Assert.That(askCount[0], Is.Zero);
        }
    }

    [Test]
    public void Perform_ScreenIsNoEditor_RunsActionWithoutAsking()
    {
        var askCount = 0;
        var guard    = new UnsavedChangesGuard(() => { askCount++; return UnsavedChangesChoice.Cancel; });
        guard.Track(new object());
        var done = false;

        guard.Perform(() => done = true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(done, Is.True);
            Assert.That(askCount, Is.Zero);
            Assert.That(guard.HasUnsavedChanges, Is.False);
        }
    }

    [Test]
    public void Perform_Cancel_DropsActionAndCallsOnCancel()
    {
        var (guard, editor, _) = Make(UnsavedChangesChoice.Cancel);
        var done      = false;
        var cancelled = false;

        guard.Perform(() => done = true, () => cancelled = true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(done, Is.False);
            Assert.That(cancelled, Is.True);
            Assert.That(editor.IsDirty, Is.True);
            Assert.That(editor.SaveCount + editor.RevertCount, Is.Zero);
        }
    }

    [Test]
    public void Perform_Discard_RevertsAndRunsAction()
    {
        var (guard, editor, _) = Make(UnsavedChangesChoice.Discard);
        var done = false;

        guard.Perform(() => done = true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(done, Is.True);
            Assert.That(editor.RevertCount, Is.EqualTo(1));
            Assert.That(editor.SaveCount, Is.Zero);
        }
    }

    [Test]
    public void Perform_Save_SavesAndRunsAction()
    {
        var (guard, editor, _) = Make(UnsavedChangesChoice.Save);
        var done = false;

        guard.Perform(() => done = true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(done, Is.True);
            Assert.That(editor.SaveCount, Is.EqualTo(1));
            Assert.That(editor.RevertCount, Is.Zero);
        }
    }

    [Test]
    public void Perform_SecondNavigationAfterDecision_DoesNotAskAgain()
    {
        // Saving can still be in progress (editor still dirty) when the second navigation of the same click runs.
        var askCount = 0;
        var editor   = new SlowEditor();
        var guard    = new UnsavedChangesGuard(() => { askCount++; return UnsavedChangesChoice.Save; });
        guard.Track(editor);
        var second = false;

        guard.Perform(() => { });
        guard.Perform(() => second = true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(askCount, Is.EqualTo(1));
            Assert.That(second, Is.True);
        }
    }

    private sealed class SlowEditor : IUnsavedChangesEditor
    {
        public bool IsDirty => true;
        public Task SaveAsync() => new TaskCompletionSource().Task;   // never completes
        public void Revert() { }
    }
}
