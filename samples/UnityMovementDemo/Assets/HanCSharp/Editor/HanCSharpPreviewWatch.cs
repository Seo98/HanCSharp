using System;

/// <summary>저장된 내용의 변화를 합쳐 한 번만 갱신한다. 파일 I/O와 Unity 이벤트는 호출자가 담당한다.</summary>
public sealed class HanCSharpPreviewWatch
{
    private string _source;
    private string _content;
    private double _due = double.PositiveInfinity;
    private readonly double _delay;

    public HanCSharpPreviewWatch(double delay = 0.35) { _delay = delay; }

    public bool Observe(string source, string content, double now, bool busy)
    {
        if (!string.Equals(source, _source, StringComparison.Ordinal))
        {
            _source = source;
            _content = content;
            _due = double.PositiveInfinity;
            return false;
        }
        if (!string.Equals(content, _content, StringComparison.Ordinal))
        {
            _content = content;
            _due = now + _delay;
        }
        if (busy || now < _due)
            return false;
        _due = double.PositiveInfinity;
        return true;
    }
}
