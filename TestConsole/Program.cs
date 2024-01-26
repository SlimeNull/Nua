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
        class NuaSinFunction : NuaFunction
        {
            public override NuaValue? Invoke(NuaContext context, params NuaValue?[] parameters)
            {
                if (parameters.Length == 0)
                    return null;
                if (parameters[0] is not NuaNumber number)
                    return null;

                return new NuaNumber(Math.Sin(number.Value));
            }
        }

        class NuaPrintFunction : NuaFunction
        {
            public override NuaValue? Invoke(NuaContext context, params NuaValue?[] parameters)
            {
                Console.WriteLine(string.Join<NuaValue?>('\t', parameters));

                return null;
            }
        }

        class NuaLenFunction : NuaFunction
        {
            public override NuaValue? Invoke(NuaContext context, params NuaValue?[] parameters)
            {
                int len = 0;
                foreach (var value in parameters)
                {
                    if (value is NuaNativeTable table)
                        len += table.Storage.Count;
                    else if (value is NuaList list)
                        len += list.Storage.Count;
                    else if (value is NuaString str)
                        len += str.Value.Length;
                }

                return new NuaNumber(len);
            }
        }

        static void Main(string[] args)
        {
            NuaContext context = new NuaContext();

            context.Set("len", new NuaLenFunction());
            context.Set("print", new NuaPrintFunction());

            context.Set("math", MathOperations.Create());
            context.Set("list", ListOperations.Create());

            Console.WriteLine("Nua TestConsole");
            Console.WriteLine();

            while (true)
            {
                Console.Write(">>> ");
                string? input = Console.ReadLine();

                if (input == null)
                    break;

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                var reader = new StringReader(input);

                try
                {
                    var tokens = Lexer.Lex(reader).ToArray();
                    var expr = Parser.Parse(tokens);

                    var result = expr?.Eval(context);

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
