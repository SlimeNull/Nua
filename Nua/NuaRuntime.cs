using System.Collections.Frozen;
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

            Context.SetGlobal("input", new NuaInputFunction());
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
                var lexer = new Lexer(reader);
                var tokens = lexer.Lex().ToArray();

                if (lexer.Status.HasError)
                    throw new NuaLexException(lexer.Status);

                var parser = new Parser(tokens);

                try
                {
                    var expr = parser.ParseMulti();
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
            var lexer = new Lexer(reader);
            var tokens = lexer.Lex().ToArray();

            if (lexer.Status.HasError)
                throw new NuaLexException(lexer.Status);

            var parser = new Parser(tokens);
            var multiExpr = parser.ParseMulti();
            var compiled = multiExpr.Compile();

            var result = compiled.Evaluate(Context);

            return result;
        }

        class NuaInputFunction : NuaFunction
        {
            public override IReadOnlyList<string> ParameterNames => ["prompt"];

            public override NuaValue? Invoke(NuaContext context, NuaValue?[] parameters, KeyValuePair<string, NuaValue?>[] namedParameters)
            {
                var namedParametersDict = namedParameters.ToFrozenDictionary();

                string? prompt = null;
                if (namedParametersDict.TryGetValue("prompt", out NuaValue? nuaPrompt))
                    prompt = nuaPrompt?.ToString();
                else if (parameters.Length > 0)
                    prompt = parameters[0]?.ToString();

                if (prompt is not null)
                    Console.Write(prompt);

                string? input = Console.ReadLine();
                return input != null ? new NuaString(input) : null;
            }
        }

        class NuaPrintFunction : NuaFunction
        {
            public override IReadOnlyList<string> ParameterNames => ["value", "...", "sep", "end"];

            public override NuaValue? Invoke(NuaContext context, NuaValue?[] parameters, KeyValuePair<string, NuaValue?>[] namedParameters)
            {
                var namedParametersDict = namedParameters.ToFrozenDictionary();

                string sep;
                if (namedParametersDict.TryGetValue("sep", out NuaValue? nuaSep))
                    sep = nuaSep?.ToString() ?? string.Empty;
                else
                    sep = "\t";

                string? end;
                if (namedParametersDict.TryGetValue("end", out NuaValue? nuaEnd))
                    end = nuaEnd?.ToString();
                else
                    end = Environment.NewLine;


                Console.Write(string.Join<NuaValue?>(sep, parameters));

                if (end is not null)
                    Console.Write(end);

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
