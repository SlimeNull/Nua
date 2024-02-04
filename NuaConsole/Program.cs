using System.Reflection;
using System.Text;
using Nua;
using Nua.CompileService;

namespace NuaConsole
{
    internal class Program
    {
        static void Main(string[] args)
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

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine($"Nua V{version}");
            Console.WriteLine();

            StringBuilder inputBuffer = new();

            while (true)
            {
                string prompt = inputBuffer.Length == 0 ? ">>> " : "... ";
                Console.Write(prompt);
                string? input = Console.ReadLine();

                if (input == null)
                    break;

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                inputBuffer.AppendLine(input);

                try
                {
                    try
                    {
                        var result = runtime.Evaluate(inputBuffer.ToString());

                        if (result != null)
                        {
                            Console.WriteLine(result);
                        }

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
