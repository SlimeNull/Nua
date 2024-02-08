using System.Text;
using Nua.CompileService;
using Nua.Stdlib;
using Nua.Types;

namespace Nua
{
    public class NuaRuntime
    {
        public NuaContext Context { get; }

        public NuaRuntime()
        {
            Context = new NuaContext();

            Context.SetGlobal("print", new NuaPrintFunction());
            Context.SetGlobal("len", new NuaLenFunction());
            Context.SetGlobal("table", TableOperations.Create());
            Context.SetGlobal("list", ListOperations.Create());
            Context.SetGlobal("math", MathOperations.Create());
            Context.SetGlobal("nua", CoreOperations.Create());
        }

        public NuaValue? Evaluate(TextReader expressionReader)
        {
            ArgumentNullException.ThrowIfNull(expressionReader, nameof(expressionReader));

            StringBuilder sb = new();
            sb.AppendLine(expressionReader.ReadLine());

            while (true)
            {
                var reader = new StringReader(sb.ToString());
                var tokens = Lexer.Lex(reader).ToArray();

                try
                {
                    var expr = Parser.ParseMulti(tokens);
                    var compiled = expr.Compile();

                    return compiled.Evaluate(Context);
                }
                catch (NuaParseException parseException) when (parseException.Status.RequireMoreTokens)
                {
                    // do nothing
                }

                var newLine = expressionReader.ReadLine();
                if (newLine == null)
                    throw new IOException();

                sb.AppendLine(newLine);
            }
        }

        public NuaValue? Evaluate(string expression)
        {
            ArgumentNullException.ThrowIfNull(expression, nameof(expression));

            var reader = new StringReader(expression);
            var tokens = Lexer.Lex(reader).ToArray();
            var multiExpr = Parser.ParseMulti(tokens);
            var compiled = multiExpr.Compile();

            var result = compiled.Evaluate(Context);

            return result;
        }

        class NuaPrintFunction : NuaFunction
        {
            public override IReadOnlyList<string> ParameterNames => ["value", "..."];

            public override NuaValue? Invoke(NuaContext context, NuaValue?[] parameters, KeyValuePair<string, NuaValue?>[] namedParameters)
            {
                Console.WriteLine(string.Join<NuaValue?>('\t', parameters));

                return null;
            }
        }

        class NuaLenFunction : NuaFunction
        {
            public override IReadOnlyList<string> ParameterNames => ["value", "..."];

            public override NuaValue? Invoke(NuaContext context, NuaValue?[] parameters, KeyValuePair<string, NuaValue?>[] namedParameters)
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
    }
}
