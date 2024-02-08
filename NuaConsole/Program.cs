using System.Reflection;
using System.Text;
using Nua;
using Nua.CompileService;
using PrettyPrompt;
using PrettyPrompt.Highlighting;

namespace NuaConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var runtime = new NuaRuntime();

            if (args.Length > 0)
            {
                foreach (var file in args)
                {
                    if (!File.Exists(file))
                        continue;

                    var content = File.ReadAllText(file);

                    runtime.Evaluate(content);
                }

                return;
            }

            if (Console.IsInputRedirected)
            {
                var input = Console.In.ReadToEnd();
                runtime.Evaluate(input);

                return;
            }

            var version = Assembly.GetExecutingAssembly().GetName().Version;

            Console.WriteLine($"Nua V{version}");
            Console.WriteLine();

            Prompt prompt = new Prompt(
                callbacks: new NuaReplPromptCallbacks(runtime.Context),
                configuration: new PromptConfiguration(
                    tabSize: 2,
                    prompt: ">>> ",
                    completionBoxBorderFormat: new ConsoleFormat(AnsiColor.White)));

            while (true)
            {
                var promptResult = await prompt.ReadLineAsync();

                if (!promptResult.IsSuccess)
                    break;

                string input = promptResult.Text;

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                try
                {
                    var result = runtime.Evaluate(input);

                    Console.WriteLine(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
