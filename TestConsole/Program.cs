using System.Diagnostics;
using Nua;
using Nua.CompileService;
using Nua.CompileService.Syntaxes;
using Nua.Stdlib;
using Nua.Types;

namespace TestConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Nua TestConsole");
            Console.WriteLine();

            NuaRuntime runtime = new();

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
