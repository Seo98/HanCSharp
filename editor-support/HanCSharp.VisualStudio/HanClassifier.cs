using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using HanCSharp.EditorSupport;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace HanCSharp.VisualStudio
{
    [Export(typeof(IClassifierProvider)), ContentType(ContentTypes.Name)]
    internal sealed class HanClassifierProvider : IClassifierProvider
    {
        [Import] internal IClassificationTypeRegistryService Registry = null;
        public IClassifier GetClassifier(ITextBuffer buffer) =>
            buffer.Properties.GetOrCreateSingletonProperty(() => new HanClassifier(buffer, Registry));
    }

    /// <summary>스냅샷마다 한 번 토큰화하고 Visual Studio의 테마 색상 분류에 연결한다.</summary>
    internal sealed class HanClassifier : IClassifier
    {
        private readonly IClassificationTypeRegistryService _registry;
        private ITextSnapshot _snapshot;
        private IList<HanToken> _tokens;
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public HanClassifier(ITextBuffer buffer, IClassificationTypeRegistryService registry)
        {
            _registry = registry;
            buffer.Changed += OnChanged;
        }
        private void OnChanged(object sender, TextContentChangedEventArgs args)
        {
            _snapshot = null;
            ClassificationChanged?.Invoke(this, new ClassificationChangedEventArgs(new SnapshotSpan(args.After, 0, args.After.Length)));
        }
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            if (_snapshot != span.Snapshot)
            {
                _snapshot = span.Snapshot;
                _tokens = HanLexer.Tokenize(_snapshot.GetText());
            }
            var result = new List<ClassificationSpan>();
            foreach (HanToken token in _tokens)
            {
                if (token.Start >= span.End.Position)
                    break;
                if (token.Start + token.Length <= span.Start.Position)
                    continue;
                string name;
                switch (token.Kind)
                {
                    case HanTokenKind.Keyword:
                        name = "keyword";
                        break;
                    case HanTokenKind.Type:
                        name = "class name";
                        break;
                    case HanTokenKind.String:
                        name = "string";
                        break;
                    case HanTokenKind.Comment:
                        name = "comment";
                        break;
                    case HanTokenKind.Number:
                        name = "number";
                        break;
                    case HanTokenKind.Fence:
                        name = "preprocessor keyword";
                        break;
                    case HanTokenKind.Operator:
                        name = "operator";
                        break;
                    default:
                        continue;
                }
                var type = _registry.GetClassificationType(name) ?? _registry.GetClassificationType("keyword");
                int begin = Math.Max(token.Start, span.Start.Position);
                int end = Math.Min(token.Start + token.Length, span.End.Position);
                result.Add(new ClassificationSpan(new SnapshotSpan(span.Snapshot, begin, end - begin), type));
            }
            return result;
        }
    }
}
