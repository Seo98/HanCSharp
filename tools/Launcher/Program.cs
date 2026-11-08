using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HanCSharp.Launcher
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            string root = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."));
            try
            {
                if (args.Length > 0)
                {
                    // WinExe에는 콘솔이 없으므로 코드 페이지를 변경하지 않고 전달받은 파이프에 UTF-8로 기록한다.
                    Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false)) { AutoFlush = true });
                    Console.SetError(new StreamWriter(Console.OpenStandardError(), new UTF8Encoding(false)) { AutoFlush = true });
                    if (args[0] == "--smoke-test")
                    {
                        Console.WriteLine(RunCompiler(root));
                        return 0;
                    }
                    if (args[0] == "--prepare-demo" && args.Length == 2)
                    {
                        Console.WriteLine(PackageOperations.PrepareDemo(root, args[1]));
                        return 0;
                    }
                    if (args[0] == "--install-unity" && args.Length >= 2)
                    {
                        PackageOperations.InstallUnity(root, args[1], Array.IndexOf(args, "--include-examples") >= 0);
                        Console.WriteLine("설치 완료: " + args[1]);
                        return 0;
                    }
                    throw new ArgumentException("지원하지 않는 시작 도구 인수입니다.");
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new StartForm(root));
                return 0;
            }
            catch (Exception exception)
            {
                if (args.Length == 0)
                    MessageBox.Show(exception.Message, "한씨 시작 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    Console.Error.WriteLine(exception.Message);
                return 1;
            }
        }

        public static string Quote(string value)
        {
            var result = new StringBuilder("\"");
            int slashes = 0;
            foreach (char c in value)
            {
                if (c == '\\')
                {
                    slashes++;
                    continue;
                }
                result.Append('\\', c == '"' ? slashes * 2 + 1 : slashes);
                result.Append(c);
                slashes = 0;
            }
            return result.Append('\\', slashes * 2).Append('"').ToString();
        }

        public static string RunCompiler(string root)
        {
            var start = new ProcessStartInfo(Path.Combine(root, "app", "HanCSharp.Cli.exe"),
                "run " + Quote(Path.Combine(root, "samples", "ConsoleGame", "Main.han")))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            using (var process = Process.Start(start))
            {
                var output = process.StandardOutput.ReadToEndAsync();
                var error = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(30000))
                {
                    process.Kill();
                    throw new TimeoutException("예제 실행 시간이 30초를 초과했습니다.");
                }
                string result = output.GetAwaiter().GetResult() + error.GetAwaiter().GetResult();
                if (process.ExitCode != 0)
                    throw new InvalidOperationException(result);
                return result;
            }
        }
    }
}
