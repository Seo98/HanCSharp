using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using HanCSharp.EditorSupport;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace HanCSharp.VisualStudio
{
    [Export(typeof(ICompletionSourceProvider)), ContentType(ContentTypes.Name), Name("HanCSharp completion")]
    internal sealed class HanCompletionSourceProvider : ICompletionSourceProvider
    {
        public ICompletionSource TryCreateCompletionSource(ITextBuffer buffer) => new HanCompletionSource(buffer);
    }

    internal sealed class HanCompletion : Completion
    {
        public CompletionInsertion Insertion
        {
            get; private set;
        }
        public HanCompletion(CompletionEntry entry, string indentation)
            : this(entry, entry.Prepare(indentation)) { }
        private HanCompletion(CompletionEntry entry, CompletionInsertion insertion)
            : base(entry.Display, insertion.Text, entry.Description, null, null)
        {
            Insertion = insertion;
        }
    }

    /// <summary>현재 버퍼의 제안만 제공하며 외부 프로세스나 파일을 읽지 않는다.</summary>
    internal sealed class HanCompletionSource : ICompletionSource
    {
        private readonly ITextBuffer _buffer;
        private bool _disposed;
        public HanCompletionSource(ITextBuffer buffer)
        {
            _buffer = buffer;
        }
        public void Dispose()
        {
            _disposed = true;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (_disposed)
                return;
            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            SnapshotPoint? point = session.GetTriggerPoint(snapshot);
            if (!point.HasValue)
                return;
            int position = point.Value.Position;
            string text = snapshot.GetText();
            IList<CompletionEntry> entries = CompletionCatalog.Suggest(text, position);
            if (entries.Count == 0)
                return;
            int start = CompletionCatalog.WordStart(text, position);
            string line = snapshot.GetLineFromPosition(position).GetText();
            string indentation = new string(line.TakeWhile(char.IsWhiteSpace).ToArray());
            ITrackingSpan span = snapshot.CreateTrackingSpan(start, position - start, SpanTrackingMode.EdgeInclusive);
            var items = entries.Select(entry => (Completion)new HanCompletion(entry, indentation)).ToList();
            completionSets.Add(new CompletionSet("HanCSharp", "한씨", span, items, null));
        }
    }
}
