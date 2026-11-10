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
        // 선언을 먼저 파악해 이전 코드의 '참/거짓' 이름을 논리 상수로 바꾸지 않는다.
        var names = BooleanLiterals.DeclaredNames(ConvertCore(source, null, null, null));
        return ConvertCore(source, sourcePath, generatedLineSources, names);
    }

    private static string ConvertCore(string source, string? sourcePath, ICollection<int>? generatedLineSources, ISet<string>? booleanNames)
    {
        var lines = SplitLines(source);
        var output = new StringBuilder();
        var blocks = new Stack<Block>();
        int currentLine = 1;
        int repeatIndex = 0;
        string Expression(string text, int line) => SyntaxRules.Expression(text, line, booleanNames);
        string Condition(string text, int line) => ParseCondition(text, line, booleanNames);

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

            string? closingKind = line switch
            {
                "조건 끝." => "condition", "반복 끝." => "loop", "함수 끝." => "function",
                "클래스 끝." or "부품 끝." => "class", "동작 끝." => "function",
                "되풀이 끝." => "loop", "이름공간 끝." => "namespace", _ => null
            };
            if (line == "끝." || closingKind != null)
            {
                if (blocks.Count == 0)
                {
                    throw new LanguageException(number, "닫을 블록이 없습니다.");
                }
                string actualKind = blocks.Peek().Kind switch
                {
                    "if" or "else" => "condition", "while" or "repeat" => "loop",
                    "method" or "function" => "function", "unity" => "class", var kind => kind
                };
                if (closingKind != null && closingKind != actualKind)
                    throw new LanguageException(number, $"'{line}'으로 {blocks.Peek().Line}행의 블록을 닫을 수 없습니다. 블록 종류를 확인하세요.");
                blocks.Pop();
                Emit(blocks.Count, "}");
                continue;
            }

            if (line is "아니면:" or "아니라면:")
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
                string condition = Condition(line[3..^1], number);
                Emit(blocks.Count, $"if ({condition}) {{");
                blocks.Push(new Block("if", number));
                continue;
            }

            if (line.EndsWith("다면:", StringComparison.Ordinal) || line.EndsWith("라면:", StringComparison.Ordinal))
            {
                Emit(blocks.Count, $"if ({Condition(line[..^1], number)}) {{");
                blocks.Push(new Block("if", number));
                continue;
            }

            if (line.StartsWith("동안 ", StringComparison.Ordinal) && line.EndsWith(':'))
            {
                string condition = Condition(line[3..^1], number);
                Emit(blocks.Count, $"while ({condition}) {{");
                blocks.Push(new Block("while", number));
                continue;
            }

            if (line.EndsWith(" 동안:", StringComparison.Ordinal))
            {
                string condition = ParseNaturalWhile(line[..^4], number, booleanNames);
                Emit(blocks.Count, $"while ({condition}) {{");
                blocks.Push(new Block("while", number));
                continue;
            }

            Match simpleRepeat = Match(line, @"^(?<count>[0-9]+)번 (?:반복|되풀이한다):$");
            if (simpleRepeat.Success)
            {
                if (!int.TryParse(simpleRepeat.Groups["count"].Value, out int count))
                    throw new LanguageException(number, "반복 횟수는 0부터 2147483647까지의 정수로 작성하세요.");
                string name;
                do { name = "__hanRepeat" + repeatIndex++; } while (source.Contains(name, StringComparison.Ordinal));
                Emit(blocks.Count, $"for (int {name} = 0; {name} < {count}; {name}++) {{");
                blocks.Push(new Block("repeat", number));
                continue;
            }

            if (NaturalSentenceSyntax.TryParse(line, number, blocks.TryPeek(out var parent) ? parent.Kind : null,
                blocks.Any(block => block.Kind == "unity"), booleanNames, out string natural, out string? naturalBlock))
            {
                Emit(blocks.Count, natural);
                if (naturalBlock != null) blocks.Push(new Block(naturalBlock, number));
                continue;
            }

            if (DeclarationSyntax.TryParse(line, number, out string declaration, out string? declarationBlock, booleanNames))
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

            Match function = Match(line, @"^함수 (?<name>\S+)\((?<args>.*)\)(?:(?:은|는) (?<type>\S+))?:$");
            if (function.Success)
            {
                string name = Identifier(function.Groups["name"].Value, number);
                string parameters = ParseParameters(function.Groups["args"].Value, number);
                string type = function.Groups["type"].Success ? Type(function.Groups["type"].Value, number, allowVoid: true) : "void";
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

    private static string ParseCondition(string text, int line, ISet<string>? booleanNames)
    {
        string Expression(string value, int number) => SyntaxRules.Expression(value, number, booleanNames);
        Match natural = Match(text, @"^(?<left>.+?)(?:이|가) (?<right>.+?)(?<op>이상이라면|이하라면|이라면|라면)$");
        if (natural.Success)
        {
            string operation = natural.Groups["op"].Value switch { "이상이라면" => ">=", "이하라면" => "<=", _ => "==" };
            return $"({Expression(natural.Groups["left"].Value, line)}) {operation} ({Expression(natural.Groups["right"].Value.Trim(), line)})";
        }
        foreach (var pair in new[] { ("크거나 같다면", "크거나 같으면"), ("작거나 같다면", "작거나 같으면"),
            ("크다면", "크면"), ("작다면", "작으면"), ("같다면", "같으면"), ("다르다면", "다르면") })
        {
            if (text.EndsWith(pair.Item1, StringComparison.Ordinal))
            {
                text = text[..^pair.Item1.Length] + pair.Item2;
                break;
            }
        }
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

    private static string ParseNaturalWhile(string text, int line, ISet<string>? booleanNames)
    {
        foreach (var pair in new[] { ("크거나 같은", "크거나 같으면"), ("작거나 같은", "작거나 같으면"),
            ("큰", "크면"), ("작은", "작으면"), ("같은", "같으면"), ("다른", "다르면") })
        {
            if (text.EndsWith(" " + pair.Item1, StringComparison.Ordinal))
                return ParseCondition(text[..^pair.Item1.Length] + pair.Item2, line, booleanNames);
        }
        if (text.EndsWith("인", StringComparison.Ordinal))
            return SyntaxRules.Expression(text[..^1], line, booleanNames);
        throw new LanguageException(line, "반복 조건은 '체력이 0보다 큰 동안:' 또는 '조건식인 동안:'처럼 작성하세요.");
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
