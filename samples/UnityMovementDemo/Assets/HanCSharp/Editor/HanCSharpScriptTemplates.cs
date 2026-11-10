using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>한씨 새 파일의 이름 검사와 기본 내용을 제공한다. 파일명과 클래스명은 동일하게 유지한다.</summary>
public static class HanCSharpScriptTemplates
{
    private static readonly HashSet<string> _reserved = new HashSet<string>(
        ("abstract as base bool break byte case catch char checked class const continue decimal default delegate do double else enum event explicit extern false finally fixed float for foreach goto if implicit in int interface internal is lock long namespace new null object operator out override params private protected public readonly ref return sbyte sealed short sizeof stackalloc static string struct switch this throw true try typeof uint ulong unchecked unsafe ushort using virtual void volatile while").Split(' '));

    public static bool IsValidName(string name)
    {
        return !string.IsNullOrEmpty(name)
            && Regex.IsMatch(name, @"^[\p{L}_][\p{L}\p{Nd}\p{Mn}\p{Mc}\p{Pc}]*$")
            && !_reserved.Contains(name);
    }

    public static string Component(string name)
    {
        if (!IsValidName(name))
            throw new ArgumentException("공백 없이 영문·한글·숫자·밑줄을 사용하고 숫자로 시작하지 마세요.", nameof(name));
        return "// 이 파일을 저장하고 Unity로 돌아오면 같은 이름의 C#이 생성됩니다.\n"
            + name + "는 유니티 부품이다:\n"
            + "    처음 시작할 때:\n"
            + "        이 물체는 \"한씨 부품이 시작되었습니다!\"를 기록한다.\n"
            + "    동작 끝.\n\n"
            + "    매 순간:\n"
            + "        // 여기에 이 물체가 할 일을 적어 주세요.\n"
            + "    동작 끝.\n부품 끝.\n";
    }
}
