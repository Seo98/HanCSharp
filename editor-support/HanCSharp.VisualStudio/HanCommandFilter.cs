using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Threading;
using HanCSharp.EditorSupport;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace HanCSharp.VisualStudio
{
    [Export(typeof(IVsTextViewCreationListener)), ContentType(ContentTypes.Name), TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class HanTextViewListener : IVsTextViewCreationListener
    {
        [Import] internal IVsEditorAdaptersFactoryService Adapters = null;
        [Import] internal ICompletionBroker Broker = null;
        public void VsTextViewCreated(IVsTextView adapter)
        {
            IWpfTextView view = Adapters.GetWpfTextView(adapter);
            if (view == null)
                return;
            view.Properties.GetOrCreateSingletonProperty(() => new HanCommandFilter(adapter, view, Broker));
        }
    }

    /// <summary>Ctrl+Space, Tab/Enter/Escape와 텍스트 변경을 완성 세션에 연결한다. IME 조합 중에는 확정하지 않는다.</summary>
    internal sealed class HanCommandFilter : IOleCommandTarget
    {
        private readonly IVsTextView _adapter;
        private readonly IWpfTextView _view;
        private readonly ICompletionBroker _broker;
        private readonly IOleCommandTarget _next;
        private ICompletionSession _session;
        private bool _pending;
        private bool _committing;
        private bool _composing;

        public HanCommandFilter(IVsTextView adapter, IWpfTextView view, ICompletionBroker broker)
        {
            _adapter = adapter;
            _view = view;
            _broker = broker;
            adapter.AddCommandFilter(this, out _next);
            view.TextBuffer.Changed += OnTextChanged;
            view.Closed += OnClosed;
            TextCompositionManager.AddPreviewTextInputStartHandler(view.VisualElement, OnCompositionStart);
            TextCompositionManager.AddPreviewTextInputHandler(view.VisualElement, OnCompositionDone);
        }

        private void OnCompositionStart(object sender, TextCompositionEventArgs args)
        {
            _composing = true;
        }
        private void OnCompositionDone(object sender, TextCompositionEventArgs args)
        {
            _composing = false;
            Schedule();
        }

        public int QueryStatus(ref Guid group, uint count, OLECMD[] commands, IntPtr text)
        {
            if (group == VSConstants.VSStd2K)
            {
                for (int index = 0; index < count; index++)
                {
                    if (commands[index].cmdID == (uint)VSConstants.VSStd2KCmdID.AUTOCOMPLETE || commands[index].cmdID == (uint)VSConstants.VSStd2KCmdID.COMPLETEWORD)
                    {
                        commands[index].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                        return VSConstants.S_OK;
                    }
                }
            }
            return _next.QueryStatus(ref group, count, commands, text);
        }

        public int Exec(ref Guid group, uint id, uint options, IntPtr input, IntPtr output)
        {
            if (group == VSConstants.VSStd2K && !_composing)
            {
                var command = (VSConstants.VSStd2KCmdID)id;
                if (command == VSConstants.VSStd2KCmdID.AUTOCOMPLETE || command == VSConstants.VSStd2KCmdID.COMPLETEWORD)
                {
                    Start();
                    return VSConstants.S_OK;
                }
                if (command == VSConstants.VSStd2KCmdID.TAB || command == VSConstants.VSStd2KCmdID.RETURN)
                {
                    if (Commit())
                        return VSConstants.S_OK;
                }
                if (command == VSConstants.VSStd2KCmdID.CANCEL)
                {
                    if (Active())
                    {
                        _session.Dismiss();
                        return VSConstants.S_OK;
                    }
                }
                if (command == VSConstants.VSStd2KCmdID.TYPECHAR && input != IntPtr.Zero)
                {
                    char typed = (char)(ushort)Marshal.GetObjectForNativeVariant(input);
                    if (!HanLexer.IsWord(typed))
                        Dismiss();
                }
            }
            return _next.Exec(ref group, id, options, input, output);
        }

        private bool Active() => _session != null && !_session.IsDismissed;
        private void Dismiss()
        {
            if (Active())
                _session.Dismiss();
        }

        private void Start()
        {
            if (_view.IsClosed || _composing)
                return;
            SnapshotPoint point = _view.Caret.Position.BufferPosition;
            if (HanLexer.IsSuppressed(point.Snapshot.GetText(), point.Position))
            {
                Dismiss();
                return;
            }
            if (Active())
            {
                FilterAndSelect();
                return;
            }
            ITrackingPoint trigger = point.Snapshot.CreateTrackingPoint(point.Position, PointTrackingMode.Positive);
            _session = _broker.CreateCompletionSession(_view, trigger, true);
            _session.Start();
            if (Active())
            {
                FilterAndSelect();
            }
        }

        // 점 바로 뒤처럼 접두어가 없을 때도 첫 제안을 Tab으로 확정할 수 있게 한다.
        private void FilterAndSelect()
        {
            _session.Filter();
            CompletionSet set = _session.SelectedCompletionSet;
            if (set == null)
                return;
            set.SelectBestMatch();
            if (!set.SelectionStatus.IsSelected && set.ApplicableTo.GetSpan(_view.TextSnapshot).Length == 0 && set.Completions.Count > 0)
                set.SelectionStatus = new CompletionSelectionStatus(set.Completions[0], true, true);
        }

        private bool Commit()
        {
            if (!Active())
                return false;
            CompletionSet set = _session.SelectedCompletionSet;
            if (set == null || !set.SelectionStatus.IsSelected)
            {
                Dismiss();
                return false;
            }
            HanCompletion selected = set.SelectionStatus.Completion as HanCompletion;
            int start = set.ApplicableTo.GetStartPoint(_view.TextSnapshot).Position;
            _committing = true;
            try
            {
                _session.Commit();
                if (selected != null && selected.Insertion.SelectionLength > 0)
                {
                    int begin = start + selected.Insertion.SelectionStart;
                    int length = selected.Insertion.SelectionLength;
                    _view.Caret.MoveTo(new SnapshotPoint(_view.TextSnapshot, begin + length));
                    _view.Selection.Select(new SnapshotSpan(_view.TextSnapshot, begin, length), false);
                }
            }
            finally { _committing = false; }
            return true;
        }

        private void OnTextChanged(object sender, TextContentChangedEventArgs args)
        {
            if (_committing || _composing)
                return;
            if (Active() || args.Changes.Any(c => c.NewText.Any(ch => HanLexer.IsWord(ch) || ch == '.')))
                Schedule();
        }

        private void Schedule()
        {
            if (_pending || _view.IsClosed || _committing)
                return;
            _pending = true;
            _view.VisualElement.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                _pending = false;
                if (_view.IsClosed || !_view.HasAggregateFocus || _composing || _committing)
                    return;
                SnapshotPoint point = _view.Caret.Position.BufferPosition;
                if (point.Position == 0)
                {
                    Dismiss();
                    return;
                }
                char previous = point.Snapshot[point.Position - 1];
                if (HanLexer.IsWord(previous) || previous == '.')
                    Start();
                else
                    Dismiss();
            }));
        }

        private void OnClosed(object sender, EventArgs args)
        {
            Dismiss();
            _view.TextBuffer.Changed -= OnTextChanged;
            _view.Closed -= OnClosed;
            TextCompositionManager.RemovePreviewTextInputStartHandler(_view.VisualElement, OnCompositionStart);
            TextCompositionManager.RemovePreviewTextInputHandler(_view.VisualElement, OnCompositionDone);
            _adapter.RemoveCommandFilter(this);
        }
    }
}
