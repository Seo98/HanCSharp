namespace HanCSharp.EditorSupport
{
    public enum HanTokenKind
    {
        Keyword, Type, String, Comment, Number, Operator, Identifier, Fence
    }

    /// <summary>편집기 버퍼의 UTF-16 위치를 나타내며 Visual Studio API에 의존하지 않는다.</summary>
    public sealed class HanToken
    {
        public int Start
        {
            get; private set;
        }
        public int Length
        {
            get; private set;
        }
        public HanTokenKind Kind
        {
            get; private set;
        }
        public HanToken(int start, int length, HanTokenKind kind)
        {
            Start = start;
            Length = length;
            Kind = kind;
        }
    }
}
