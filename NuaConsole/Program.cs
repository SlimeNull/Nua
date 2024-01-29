using System.Reflection;
using Nua;

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

            while (true)
            {
                Console.Write(">>> ");
                string? input = Console.ReadLine();

                if (input == null)
                    break;

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                try
                {
                    var result = runtime.Evaluate(input);

                    if (result != null)
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
