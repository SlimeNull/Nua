using System.Reflection;
using System.Text;
using Nua;
using Nua.CompileService;
using PrettyPrompt;

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

            StringBuilder inputBuffer = new();

            Prompt prompt = new Prompt(
                callbacks: new NuaReplPromptCallbacks(),
                configuration: new PromptConfiguration(
                    tabSize: 2,
                    prompt: ">>> "));

            while (true)
            {
                //string prompt = inputBuffer.Length == 0 ? ">>> " : "... ";
                //Console.Write(prompt);
                var promptResult = await prompt.ReadLineAsync();

                if (!promptResult.IsSuccess)
                    break;

                string input = promptResult.Text;

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                inputBuffer.AppendLine(input);

                try
                {
                    try
                    {
                        var result = runtime.Evaluate(inputBuffer.ToString());

                        Console.WriteLine(result);
                        inputBuffer.Clear();
                    }
                    catch (NuaParseException parseException)
                    {
                        if (!parseException.Status.RequireMoreTokens)
                        {
                            inputBuffer.Clear();
                            throw parseException;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    inputBuffer.Clear();
                }
            }
        }
    }
}
