using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HanCSharp.Launcher
{
    /// <summary>처음 사용자의 예제 실행·안전한 데모 준비·설치 진입을 제공한다.</summary>
    internal sealed class StartForm : Form
    {
        private readonly string _root;
        private readonly TextBox _output = new TextBox();
        private readonly FlowLayoutPanel _buttons = new FlowLayoutPanel();

        public StartForm(string root)
        {
            _root = root;
            Text = "한씨 시작하기";
            ClientSize = new Size(680, 510);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("맑은 고딕", 10f);
            var heading = new Label { Text = "한씨 — 한국어로 C#과 Unity 시작하기\r\nUnity 예제는 다운로드 위치와 별개인 짧은 경로에 준비합니다.", Dock = DockStyle.Top, Height = 65, Padding = new Padding(16, 12, 0, 0) };
            _buttons.Dock = DockStyle.Top;
            _buttons.Height = 185;
            _buttons.Padding = new Padding(12, 5, 12, 5);
            AddButton("① 한씨 콘솔 예제 실행", () => RunAction(() => Program.RunCompiler(_root)));
            AddButton("② Unity 이동 예제 준비·실행", () => OpenDemo(false));
            AddButton("③ 기존 Unity 프로젝트에 설치", InstallUnity);
            AddButton("Visual Studio 확장 설치", () => OpenFile(Path.Combine(_root, "visualstudio", "HanCSharp-0.3.vsix")));
            AddButton("처음 사용자 가이드", ShowGuide);
            AddButton("예제 저장 위치 선택", () => OpenDemo(true));
            _output.Multiline = true;
            _output.ReadOnly = true;
            _output.ScrollBars = ScrollBars.Vertical;
            _output.Dock = DockStyle.Fill;
            Controls.Add(_output);
            Controls.Add(_buttons);
            Controls.Add(heading);
            Shown += async (sender, args) => await RunAction(() => Program.RunCompiler(_root));
        }

        private void AddButton(string text, Func<Task> action)
        {
            var button = new Button { Text = text, Width = 312, Height = 47, Margin = new Padding(5) };
            button.Click += async (sender, args) => await action();
            _buttons.Controls.Add(button);
        }

        private async Task RunAction(Func<string> action)
        {
            _buttons.Enabled = false;
            UseWaitCursor = true;
            _output.Text = "처리 중입니다…";
            try
            {
                _output.Text = await Task.Run(action);
            }
            catch (Exception exception)
            {
                _output.Text = exception.Message;
            }
            finally
            {
                _buttons.Enabled = true;
                UseWaitCursor = false;
            }
        }

        private async Task OpenDemo(bool chooseLocation)
        {
            string destination = null;
            if (!chooseLocation)
            {
                try
                {
                    destination = PackageOperations.DefaultDemoPath(_root);
                }
                catch (IOException)
                {
                    _output.Text = "짧은 예제 저장 경로를 선택하세요.";
                }
            }
            if (destination == null)
            {
                using (var dialog = new FolderBrowserDialog { Description = "짧은 경로에 예제를 준비할 상위 폴더를 선택하세요." })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;
                    destination = Path.Combine(dialog.SelectedPath, "HanCSharpDemo-" + File.ReadAllText(Path.Combine(_root, "VERSION")).Trim());
                }
            }
            string prepared = null;
            await RunAction(() =>
            {
                prepared = PackageOperations.PrepareDemo(_root, destination);
                return "예제 준비 완료: " + prepared;
            });
            if (prepared == null)
                return;
            string version = File.ReadAllText(Path.Combine(prepared, "ProjectSettings", "ProjectVersion.txt")).Split('\n')[0].Split(':')[1].Trim();
            string editor = UnityLocator.Find(version);
            if (editor == null)
            {
                using (var dialog = new OpenFileDialog { Title = "Unity " + version + "의 Editor/Unity.exe를 선택하세요", Filter = "Unity Editor|Unity.exe" })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        _output.AppendText("\r\nUnity Hub에서 이 프로젝트 폴더를 추가해 열 수도 있습니다.");
                        return;
                    }
                    editor = dialog.FileName;
                }
            }
            try
            {
                Process.Start(new ProcessStartInfo(editor, "-projectPath " + Program.Quote(prepared)) { UseShellExecute = false });
                _output.AppendText("\r\nUnity를 여는 중입니다. 컴파일 후 이동 씬이 자동으로 열립니다. Play → Game 화면 클릭 → WASD/방향키로 이동하세요.");
            }
            catch (Exception exception)
            {
                _output.AppendText("\r\n" + exception.Message);
            }
        }

        private async Task InstallUnity()
        {
            using (var dialog = new FolderBrowserDialog { Description = "Assets와 ProjectSettings가 있는 Unity 프로젝트 루트를 선택하세요.", ShowNewFolderButton = false })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                string project = dialog.SelectedPath;
                await RunAction(() =>
                {
                    PackageOperations.InstallUnity(_root, project, true);
                    return "설치 완료: " + project + "\r\nUnity로 돌아가 컴파일을 기다린 뒤 Create → HanCSharp를 사용하세요.";
                });
            }
        }

        private Task OpenFile(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception exception)
            {
                _output.Text = exception.Message;
            }
            return Task.CompletedTask;
        }

        private Task ShowGuide()
        {
            using (var guide = new Form { Text = "한씨 처음 사용자 가이드", Size = new Size(900, 680), StartPosition = FormStartPosition.CenterParent })
            {
                guide.Controls.Add(new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = Font, Text = File.ReadAllText(Path.Combine(_root, "docs", "QUICKSTART.md")) });
                guide.ShowDialog(this);
            }
            return Task.CompletedTask;
        }
    }
}
