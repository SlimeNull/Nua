using System.Diagnostics;
using Nua;
using Nua.CompileService;
using Nua.CompileService.Syntaxes;
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
                    if (value is NuaDictionary dict)
                        len += dict.Storage.Count;
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

            context.Set("sin", new NuaSinFunction());
            context.Set("len", new NuaLenFunction());
            context.Set("print", new NuaPrintFunction());

            Stopwatch sw = new();

            while (true)
            {
                Console.Write(">>> ");
                string? input = Console.ReadLine();

                if (input == null)
                    break;

                var reader = new StringReader(input);

                try
                {
                    var tokens = Lexer.Lex(reader).ToArray();
                    var expr = Parser.Parse(tokens);

                    var result = expr?.Eval(context);

                    Console.WriteLine(expr);

                    if (result != null)
                    {
                        if (result is NuaString str)
                            Console.WriteLine(str.Value);
                        else if (result is NuaNumber number)
                            Console.WriteLine(number.Value);
                        else if (result is NuaBoolean boolean)
                            Console.WriteLine(boolean.Value);
                        else
                            Console.WriteLine(result);
                    }
                    else
                    {
                        Console.WriteLine("null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
