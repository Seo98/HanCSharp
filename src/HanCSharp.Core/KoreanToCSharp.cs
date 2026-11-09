using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HanCSharp;

/// <summary>
/// 한국어 문장을 C# 소스로 변환한다. 입출력과 실행은 담당하지 않는다.
/// 식은 Roslyn으로 검증하고, 원문 블록은 내용 그대로 전달한다.
/// </summary>
public sealed class KoreanToCSharp
{
    private sealed record SourceLine(string Text, string Ending);
    private sealed record Block(string Kind, int Line);

    public string Convert(string source, string? sourcePath = null, ICollection<int>? generatedLineSources = null)
    {
        var lines = SplitLines(source);
        var output = new StringBuilder();
        var blocks = new Stack<Block>();
        int currentLine = 1;

        void Emit(int depth, string text)
        {
            if (sourcePath is not null)
            {
                output.Append("#line ").Append(currentLine).Append(' ')
                    .Append('"').Append(Path.GetFullPath(sourcePath)).Append('"').Append('\n');
                generatedLineSources?.Add(0);
            }
            Append(output, depth, text);
            generatedLineSources?.Add(currentLine);
        }

        for (int index = 0; index < lines.Count; index++)
        {
            string line = lines[index].Text.Trim();
            int number = index + 1;
            currentLine = number;

            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
            {
                Emit(blocks.Count, line);
                continue;
            }

            if (Regex.IsMatch(line, "^`{3,}csharp$"))
            {
                string fence = line[..^6];
                bool closed = false;
                if (sourcePath is not null)
                {
                    output.Append("#line ").Append(number + 1).Append(' ')
                        .Append('"').Append(Path.GetFullPath(sourcePath)).Append('"').Append('\n');
                    generatedLineSources?.Add(0);
                }
                while (++index < lines.Count)
                {
                    if (lines[index].Text.Trim() == fence)
                    {
                        closed = true;
                        break;
                    }
                    output.Append(lines[index].Text).Append(lines[index].Ending);
                    generatedLineSources?.Add(index + 1);
                }
                if (!closed)
                {
                    throw new LanguageException(number, "C# 원문 블록의 닫는 표시가 없습니다.");
                }
                continue;
            }

            if (line == "끝.")
            {
                if (blocks.Count == 0)
                {
                    throw new LanguageException(number, "닫을 블록이 없습니다.");
                }
                blocks.Pop();
                Emit(blocks.Count, "}");
                continue;
            }

            if (line == "아니면:")
            {
                if (blocks.Count == 0 || blocks.Peek().Kind != "if")
                {
                    throw new LanguageException(number, "'아니면:'은 '만약' 블록에서 한 번만 쓸 수 있습니다.");
                }
                blocks.Pop();
                Emit(blocks.Count, "} else {");
                blocks.Push(new Block("else", number));
                continue;
            }

            if (line.StartsWith("만약 ", StringComparison.Ordinal) && line.EndsWith(':'))
            {
                string condition = ParseCondition(line[3..^1], number);
                Emit(blocks.Count, $"if ({condition}) {{");
                blocks.Push(new Block("if", number));
                continue;
            }

            if (line.StartsWith("동안 ", StringComparison.Ordinal) && line.EndsWith(':'))
            {
                string condition = ParseCondition(line[3..^1], number);
                Emit(blocks.Count, $"while ({condition}) {{");
                blocks.Push(new Block("while", number));
                continue;
            }

            if (DeclarationSyntax.TryParse(line, number, out string declaration, out string? declarationBlock))
            {
                Emit(blocks.Count, declaration);
                if (declarationBlock is not null) blocks.Push(new Block(declarationBlock, number));
                continue;
            }

            if (line.StartsWith("기다린다 ", StringComparison.Ordinal) && line.EndsWith('.'))
            {
                Emit(blocks.Count, $"yield return {Expression(line[5..^1], number)};");
                continue;
            }
            if (line == "양보종료.")
            {
                Emit(blocks.Count, "yield break;");
                continue;
            }
            if (line.StartsWith("기록 ", StringComparison.Ordinal) && line.EndsWith('.'))
            {
                Emit(blocks.Count, $"global::UnityEngine.Debug.Log({Expression(line[3..^1], number)});");
                continue;
            }

            Match repeat = Match(line, @"^반복 (?<name>\S+?)(?:은|는) (?<count>.+)회:$");
            if (repeat.Success)
            {
                string name = Identifier(repeat.Groups["name"].Value, number);
                string count = Expression(repeat.Groups["count"].Value, number);
                Emit(blocks.Count, $"for (int {name} = 0; {name} < ({count}); {name}++) {{");
                blocks.Push(new Block("repeat", number));
                continue;
            }

            Match function = Match(line, @"^함수 (?<name>\S+)\((?<args>.*)\)(?:은|는) (?<type>\S+):$");
            if (function.Success)
            {
                string name = Identifier(function.Groups["name"].Value, number);
                string parameters = ParseParameters(function.Groups["args"].Value, number);
                string type = Type(function.Groups["type"].Value, number, allowVoid: true);
                Emit(blocks.Count, $"{type} {name}({parameters}) {{");
                blocks.Push(new Block("function", number));
                continue;
            }

            Match variable = Match(line, @"^(?<type>\S+) (?<name>\S+?)(?:은|는) (?<value>.+)이다\.$");
            if (variable.Success)
            {
                string type = Type(variable.Groups["type"].Value, number);
                string name = Identifier(variable.Groups["name"].Value, number);
                string value = Expression(variable.Groups["value"].Value, number);
                Emit(blocks.Count, $"{type} {name} = {value};");
                continue;
            }

            Match change = Match(line, @"^(?<name>\S+)에 (?<value>.+)(?:을|를) (?<verb>넣는다|더한다|곱한다)\.$");
            if (!change.Success)
            {
                change = Match(line, @"^(?<name>\S+)에서 (?<value>.+)(?:을|를) (?<verb>뺀다)\.$");
            }
            if (change.Success)
            {
                string name = Identifier(change.Groups["name"].Value, number);
                string value = Expression(change.Groups["value"].Value, number);
                string operation = change.Groups["verb"].Value switch
                {
                    "더한다" => "+=", "뺀다" => "-=", "곱한다" => "*=", _ => "="
                };
                Emit(blocks.Count, $"{name} {operation} {value};");
                continue;
            }

            if (line.StartsWith("출력 ", StringComparison.Ordinal) && line.EndsWith('.'))
            {
                string value = Expression(line[3..^1], number);
                Emit(blocks.Count, $"global::System.Console.WriteLine({value});");
                continue;
            }

            if (line == "반환." || line.StartsWith("반환 ", StringComparison.Ordinal) && line.EndsWith('.'))
            {
                string statement = line == "반환." ? "return;" : $"return {Expression(line[3..^1], number)};";
                Emit(blocks.Count, statement);
                continue;
            }

            if (line is "중단." or "계속.")
            {
                Emit(blocks.Count, line == "중단." ? "break;" : "continue;");
                continue;
            }

            if (line.StartsWith("실행 ", StringComparison.Ordinal) && line.EndsWith('.'))
            {
                string expression = Expression(line[3..^1], number);
                var statement = SyntaxFactory.ParseStatement(expression + ";");
                if (statement.ContainsDiagnostics)
                {
                    throw new LanguageException(number, "실행문으로 사용할 수 없는 식입니다.");
                }
                Emit(blocks.Count, expression + ";");
                continue;
            }

            throw new LanguageException(number, "알 수 없는 문장입니다. 마침표, 조사, ':'를 확인하세요. C#은 원문 블록에 넣을 수 있습니다.");
        }

        if (blocks.Count > 0)
        {
            Block open = blocks.Peek();
            throw new LanguageException(open.Line, "이 블록을 닫는 '끝.'이 없습니다.");
        }
        return output.ToString();
    }

    private static string ParseCondition(string text, int line)
    {
        Match comparison = Match(text, @"^(?<left>.+?)(?:이|가) (?<right>.+)보다 (?<op>크면|작으면|크거나 같으면|작거나 같으면)$");
        if (!comparison.Success)
        {
            comparison = Match(text, @"^(?<left>.+?)(?:이|가) (?<right>.+)(?:와|과) (?<op>같으면|다르면)$");
        }
        if (comparison.Success)
        {
            string left = Expression(comparison.Groups["left"].Value, line);
            string right = Expression(comparison.Groups["right"].Value, line);
            string operation = comparison.Groups["op"].Value switch
            {
                "크면" => ">", "작으면" => "<", "크거나 같으면" => ">=",
                "작거나 같으면" => "<=", "같으면" => "==", _ => "!="
            };
            return $"({left}) {operation} ({right})";
        }
        if (text.EndsWith("이면", StringComparison.Ordinal))
        {
            return Expression(text[..^2], line);
        }
        throw new LanguageException(line, "조건은 'a가 0보다 크면' 또는 'C#조건식이면' 형식으로 작성하세요.");
    }

    private static string ParseParameters(string text, int line) => SyntaxRules.Parameters(text, line);
    private static string Type(string text, int line, bool allowVoid = false) => SyntaxRules.Type(text, line, allowVoid);
    private static string Identifier(string text, int line) => SyntaxRules.Identifier(text, line);
    private static string Expression(string text, int line) => SyntaxRules.Expression(text, line);

    private static Match Match(string text, string pattern) =>
        Regex.Match(text, pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private static void Append(StringBuilder output, int depth, string line) =>
        output.Append(' ', depth * 4).Append(line).Append('\n');

    private static List<SourceLine> SplitLines(string text)
    {
        var lines = new List<SourceLine>();
        foreach (Match match in Regex.Matches(text, @"([^\r\n]*)(\r\n|\r|\n|$)"))
        {
            if (match.Length > 0)
            {
                lines.Add(new SourceLine(match.Groups[1].Value, match.Groups[2].Value));
            }
        }
        return lines;
    }
}
