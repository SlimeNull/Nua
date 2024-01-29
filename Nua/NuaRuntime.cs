using Nua.CompileService;
using Nua.Stdlib;
using Nua.Types;

namespace Nua
{
    public class NuaRuntime
    {
        readonly NuaContext rootContext;
        public NuaContext Context { get; }

        public NuaRuntime()
        {
            rootContext = new()
            {
                Values =
                {
                    ["print"] = new NuaPrintFunction(),
                    ["len"] = new NuaLenFunction(),
                    ["table"] = TableOperations.Create(),
                    ["list"] = ListOperations.Create(),
                }
            };

            Context = new NuaContext(rootContext);
        }

        public NuaValue? Eval(string expression)
        {
            ArgumentNullException.ThrowIfNull(expression, nameof(expression));

            var reader = new StringReader(expression);
            var tokens = Lexer.Lex(reader).ToArray();
            var multiExpr = Parser.ParseMulti(tokens);

            var result = multiExpr.Eval(Context);

            return result;
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
    }
}
