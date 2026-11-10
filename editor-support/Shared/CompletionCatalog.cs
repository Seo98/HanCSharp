using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HanCSharp.EditorSupport
{
    /// <summary>한씨 문법·문장 템플릿·문서 선언 이름을 제안한다. C# 타입 추론 엔진은 아니다.</summary>
    public static class CompletionCatalog
    {
        private static readonly string[] _types = {
            "정수", "긴정수", "실수", "단정도실수", "실수32", "실수64", "십진수", "문자열", "문자", "논리", "객체", "자동", "없음",
            "벡터2", "벡터3", "회전", "색상", "코루틴"
        };
        private static readonly string[] _modifiers = { "공개", "비공개", "보호", "내부", "정적", "읽기전용", "상수", "가상", "재정의", "추상", "봉인", "부분", "비동기" };
        private static readonly CompletionEntry[] _entries = CreateEntries();
        public static IEnumerable<CompletionEntry> Entries => _entries;

        private static CompletionEntry[] CreateEntries()
        {
            var result = new List<CompletionEntry>();
            Add(result, "쉬운 값 만들기", "«체력»은 100이다.", "이름을 먼저 적고 처음 값을 적습니다. 값의 종류는 자동으로 정합니다.");
            Add(result, "쉬운 출력", "컴퓨터는 «\"안녕!\"»을 출력한다.", "컴퓨터(주어) → 보여줄 값(목적어) → 출력한다(동사) 순서입니다. 콘솔에 한 줄을 씁니다.");
            Add(result, "쉬운 조건", "«체력»이 0보다 크다면:\n    컴퓨터는 \"아직 괜찮아!\"를 출력한다.\n아니라면:\n    컴퓨터는 \"쉬어야 해!\"를 출력한다.\n조건 끝.", "조건을 먼저 말하고 그때 할 일을 적습니다. 체력은 먼저 만들어 주세요.");
            Add(result, "쉬운 되풀이", "3번 되풀이한다:\n    컴퓨터는 «\"안녕!\"»을 출력한다.\n되풀이 끝.", "안쪽의 일을 세 번 합니다.");
            Add(result, "쉬운 동작 만들기", "«인사하기»는 이렇게 한다:\n    컴퓨터는 \"안녕!\"을 출력한다.\n동작 끝.\n인사하기를 실행한다.", "여러 일을 한 이름으로 묶고 이름을 불러 실행합니다.");
            Add(result, "쉬운 유니티 부품", "«HanEasyMover»는 유니티 부품이다:\n    조절할 이동속도는 4이다.\n\n    처음 시작할 때:\n        이 물체는 \"출발!\"을 기록한다.\n    동작 끝.\n\n    매 순간:\n        이 물체는 오른쪽으로 초당 이동속도만큼 움직인다.\n    동작 끝.\n부품 끝.", "파일명과 부품 이름을 맞추고 물체에 붙이세요. 세계 좌표의 오른쪽(+X)으로 움직입니다.");
            foreach (string type in _types)
                result.Add(new CompletionEntry(type, type, "자료형입니다. 변수·필드·인수·반환형 위치에서 사용합니다."));
            foreach (string modifier in _modifiers)
                result.Add(new CompletionEntry(modifier, modifier, "클래스·멤버의 접근 범위 또는 동작을 지정하는 수식어입니다."));
            string[] keywords = { "가져오기", "이름공간", "클래스", "필드", "속성", "이벤트", "함수", "특성", "만약", "아니면", "동안", "반복", "끝.", "반환", "중단.", "계속.", "실행", "출력", "기록", "기다린다", "양보종료.", "넣는다.", "더한다.", "뺀다.", "곱한다.", "크면:", "작으면:", "같으면:", "다르면:" };
            foreach (string keyword in keywords)
                result.Add(new CompletionEntry(keyword, keyword, "한씨 문법. 자세한 예제는 개발 가이드의 문법표를 참고하세요."));
            foreach (string keyword in new[] { "참", "거짓", "조건 끝.", "반복 끝.", "함수 끝.", "클래스 끝.", "이름공간 끝." })
                result.Add(new CompletionEntry(keyword, keyword, "v0.7 문법. 기존 true/false 및 끝. 표기도 계속 사용할 수 있습니다."));
            Add(result, "논리 변수", "논리 «활성»은 참이다.", "한국어 논리값으로 초기화합니다.");
            Add(result, "간단 반복", "3번 반복:\n    출력 «\"반복\"».\n반복 끝.", "번호가 필요 없을 때 정해진 횟수만큼 반복합니다.");
            Add(result, "동안 반복", "«체력»이 0보다 큰 동안:\n    체력에서 1을 뺀다.\n반복 끝.", "조건이 유지되는 동안 반복합니다. 이름을 바꾸면 본문 참조도 수정하세요.");
            Add(result, "변수 선언", "정수 «체력»은 100이다.", "정수 변수를 선언하고 초기값을 저장합니다.");
            Add(result, "문자열 선언", "문자열 «이름»은 \"플레이어\"이다.", "문자열 변수를 선언합니다.");
            Add(result, "조건문", "만약 «조건»이면:\n    출력 \"참입니다\".\n끝.", "조건식을 확인합니다. 선택된 '조건'을 bool 식으로 바꾸세요.");
            Add(result, "조건문 아니면", "만약 «조건»이면:\n    출력 \"참입니다\".\n아니면:\n    출력 \"거짓입니다\".\n끝.", "참과 거짓의 두 흐름을 작성합니다.");
            Add(result, "횟수 반복", "반복 «차례»는 4회:\n    출력 차례.\n끝.", "0부터 3까지 반복합니다. 이름을 바꾸면 본문의 참조도 함께 수정하세요.");
            Add(result, "지역 함수", "함수 «더하기»(정수 x, 정수 y)는 정수:\n    반환 x + y.\n끝.", "반환값이 있는 지역 함수입니다.");
            Add(result, "멤버 함수", "공개 함수 «TakeDamage»(정수 amount):\n    실행 System.Console.WriteLine(amount).\n끝.", "클래스 안에서 작성하는 공개 메서드입니다.");
            Add(result, "클래스 틀", "공개 봉인 클래스 «PlayerState»:\n    비공개 필드 정수 _health는 100이다.\n    공개 속성 정수 Health는 _health이다.\n끝.", "상태 필드와 읽기 속성을 갖는 클래스입니다.");
            Add(result, "콘솔 프로그램", "공개 클래스 «Program»:\n    공개 정적 함수 Main():\n        출력 \"안녕하세요, 한씨!\".\n    끝.\n끝.", "파일 최상위에 삽입합니다. 프로그램에는 Main을 하나만 두세요.");
            Add(result, "유니티 컴포넌트", "가져오기 UnityEngine.\n\n공개 봉인 클래스 «HanComponent»는 MonoBehaviour를 상속한다:\n    특성 SerializeField.\n    비공개 필드 실수32 _speed는 90f이다.\n\n    비공개 함수 Update():\n        실행 transform.Rotate(Vector3.up, _speed * Time.deltaTime).\n    끝.\n끝.", "파일 최상위에서 사용하며 클래스명과 .han 파일명을 일치시키세요.");
            Add(result, "직렬화 필드", "특성 SerializeField.\n비공개 필드 실수32 «_moveSpeed»는 3f이다.", "Unity Inspector에서 조정할 private 필드입니다.");
            Add(result, "유니티 Update", "비공개 함수 Update():\n    기록 «\"Update\"».\n끝.", "MonoBehaviour 클래스 안에서 사용합니다. 매 프레임 로그 예제는 확인 후 제거하세요.");
            Add(result, "유니티 코루틴", "비공개 함수 «Start»()는 코루틴:\n    기다린다 new UnityEngine.WaitForSeconds(1f).\n    기록 \"1초 경과\".\n끝.", "MonoBehaviour 클래스 안의 IEnumerator 콜백 예제입니다.");
            Add(result, "C# 원문 블록", "```csharp\n«// C# 코드를 작성하세요.»\n```", "한국어 문법이 없는 C# 구문을 현재 위치에 그대로 삽입합니다.");
            string[] apis = { "UnityEngine", "System", "System.Collections.Generic", "SerializeField", "Range", "Header", "CreateAssetMenu", "MonoBehaviour", "ScriptableObject", "GameObject", "Transform", "Vector2", "Vector3", "Quaternion", "Color", "Rigidbody", "Collider", "WaitForSeconds", "IEnumerator", "Action", "List", "Dictionary", "Start", "Update", "FixedUpdate", "OnEnable", "OnDisable", "OnCollisionEnter", "true", "false", "null", "new", "this", "await", "nameof" };
            foreach (string api in apis)
                result.Add(new CompletionEntry(api, api, "C#/Unity 이름은 원래 표기를 사용합니다. 실제 사용에는 해당 참조와 선언이 필요합니다."));
            return result.ToArray();
        }

        private static void Add(List<CompletionEntry> items, string display, string code, string description) =>
            items.Add(new CompletionEntry(display, code, description, true));

        public static IList<CompletionEntry> Suggest(string text, int position)
        {
            position = Math.Max(0, Math.Min(position, text.Length));
            if (HanLexer.IsSuppressed(text, position))
                return new List<CompletionEntry>();
            int start = WordStart(text, position);
            string before = text.Substring(0, start);
            var members = MemberSuggestions(before);
            if (members != null)
                return members;
            return _entries.Concat(FindSymbols(text)).GroupBy(e => e.Display, StringComparer.Ordinal)
                .Select(g => g.First()).OrderBy(e => e.Display, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static int WordStart(string text, int position)
        {
            while (position > 0 && HanLexer.IsWord(text[position - 1]))
                position--;
            return position;
        }

        private static IList<CompletionEntry> MemberSuggestions(string before)
        {
            string owner;
            string[] members;
            Match match = Regex.Match(before, @"(?<owner>transform|Time|Vector3|Vector2|Mathf|Debug|gameObject)\.$");
            if (!match.Success)
                return null;
            owner = match.Groups["owner"].Value;
            switch (owner)
            {
                case "transform":
                    members = new[] { "position", "localPosition", "rotation", "localRotation", "localScale", "forward", "right", "up", "Rotate", "Translate", "SetParent" };
                    break;
                case "Time":
                    members = new[] { "deltaTime", "fixedDeltaTime", "time", "unscaledDeltaTime", "timeScale" };
                    break;
                case "Vector3":
                    members = new[] { "zero", "one", "up", "down", "left", "right", "forward", "back", "Distance", "Lerp" };
                    break;
                case "Vector2":
                    members = new[] { "zero", "one", "up", "down", "left", "right", "Distance", "Lerp" };
                    break;
                case "Mathf":
                    members = new[] { "Clamp", "Clamp01", "Max", "Min", "Abs", "Lerp", "MoveTowards" };
                    break;
                case "Debug":
                    members = new[] { "Log", "LogWarning", "LogError", "Assert", "DrawRay" };
                    break;
                default:
                    members = new[] { "SetActive", "activeSelf", "name", "transform", "GetComponent" };
                    break;
            }
            return members.Select(m => new CompletionEntry(m, m, owner + "." + m + " — 자주 쓰는 Unity API 목록입니다. 인수와 실제 타입은 C# 컴파일러가 검사합니다.")).ToList();
        }

        private static IEnumerable<CompletionEntry> FindSymbols(string text)
        {
            char[] clean = text.ToCharArray();
            foreach (HanToken token in HanLexer.Tokenize(text))
            {
                if (token.Kind != HanTokenKind.String && token.Kind != HanTokenKind.Comment && token.Kind != HanTokenKind.Fence)
                    continue;
                for (int index = token.Start; index < token.Start + token.Length; index++)
                {
                    if (clean[index] != '\n' && clean[index] != '\r')
                        clean[index] = ' ';
                }
            }
            string source = new string(clean);
            const string identifier = @"@?[\p{L}_][\p{L}\p{N}_]*?";
            foreach (string sentence in source.Split('\n'))
            {
                Match natural = Regex.Match(sentence.Trim(), @"^(?:조절할 )?(?<name>" + identifier + @")(?:은|는) (?:.+이다\.|이렇게 한다:)$");
                if (natural.Success && natural.Groups["name"].Value != "컴퓨터")
                    yield return Symbol(natural.Groups["name"].Value, "문장으로 만든 값 또는 동작");
                Match component = Regex.Match(sentence.Trim(), @"^(?<name>" + identifier + @")(?:은|는) 유니티 부품이다:$");
                if (component.Success) yield return Symbol(component.Groups["name"].Value, "유니티 부품");
            }
            foreach (Match match in Regex.Matches(source, @"\b(?:클래스|함수)\s+(?<name>" + identifier + @")(?=\(|:|(?:은|는)\s)"))
                yield return Symbol(match.Groups["name"].Value, "클래스 또는 함수");
            foreach (string line in source.Split('\n'))
            {
                string candidate = line.Trim();
                if (candidate.EndsWith(" 동안:", StringComparison.Ordinal) || candidate.EndsWith(" 끝.", StringComparison.Ordinal)
                    || Regex.IsMatch(candidate, @"^[0-9]+번 반복:$"))
                    continue;
                string[] parts = candidate.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int index = 0;
                while (index < parts.Length && _modifiers.Contains(parts[index]))
                    index++;
                if (index < parts.Length && new[] { "필드", "속성", "이벤트" }.Contains(parts[index]))
                    index++;
                if (parts.Length > index + 1 && !new[] { "만약", "동안", "함수", "클래스", "가져오기", "이름공간", "반환", "출력", "실행", "기록", "특성" }.Contains(parts[index]))
                {
                    Match declaration = Regex.Match(string.Join(" ", parts.Skip(index)), @"^\S+\s+(?<name>" + identifier + @")(?:(?:은|는)\s|\.$)");
                    if (declaration.Success)
                        yield return Symbol(declaration.Groups["name"].Value, "변수·필드·속성·이벤트 또는 반복 변수");
                }
            }
            foreach (Match function in Regex.Matches(source, @"함수\s+\S+\((?<args>[^\r\n]*)\)(?:(?:은|는)|:)"))
            {
                foreach (Match parameter in Regex.Matches(function.Groups["args"].Value, @"\S+\s+(?<name>" + identifier + @")(?=,|$)"))
                    yield return Symbol(parameter.Groups["name"].Value, "매개변수");
            }
        }

        private static CompletionEntry Symbol(string name, string kind) =>
            new CompletionEntry(name, name, "현재 문서에서 선언한 " + kind + "입니다. 범위와 타입을 추론한 결과는 아닙니다.");
    }
}
