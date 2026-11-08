using System;

namespace HanCSharp.EditorSupport
{
    /// <summary>완성 항목의 설명·삽입 코드·템플릿에서 선택할 이름을 보관한다.</summary>
    public sealed class CompletionEntry
    {
        public string Display
        {
            get; private set;
        }
        public string Template
        {
            get; private set;
        }
        public string Description
        {
            get; private set;
        }
        public bool IsTemplate
        {
            get; private set;
        }
        public CompletionEntry(string display, string template, string description, bool isTemplate = false)
        {
            Display = display;
            Template = template;
            Description = description;
            IsTemplate = isTemplate;
        }
        public CompletionInsertion Prepare(string indentation)
        {
            string result = Template.Replace("\n", Environment.NewLine + indentation);
            int start = result.IndexOf('«');
            int end = result.IndexOf('»');
            int length = start >= 0 && end > start ? end - start - 1 : 0;
            if (length > 0)
                result = result.Remove(end, 1).Remove(start, 1);
            return new CompletionInsertion(result, start, length);
        }
    }

    public sealed class CompletionInsertion
    {
        public string Text
        {
            get; private set;
        }
        public int SelectionStart
        {
            get; private set;
        }
        public int SelectionLength
        {
            get; private set;
        }
        public CompletionInsertion(string text, int start, int length)
        {
            Text = text;
            SelectionStart = start;
            SelectionLength = length;
        }
    }
}
