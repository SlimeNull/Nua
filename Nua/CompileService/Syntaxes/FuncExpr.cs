using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class FuncExpr : ValueExpr
    {
        public IReadOnlyList<string> ParameterNames { get; }
        public MultiExpr? Body { get; }

        public FuncExpr(IEnumerable<string> parameterNames, MultiExpr? body)
        {
            ParameterNames = parameterNames.ToList().AsReadOnly();
            Body = body;
        }

        public override NuaValue? Eval(NuaContext context)
            => new NuaNativeFunction(Body, ParameterNames.ToArray());

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;
            if (!TokenMatch(tokens, ref cursor, TokenKind.KwdFunction, out _))
                return false;

            if (!TokenMatch(tokens, ref cursor, TokenKind.ParenthesesLeft, out _))
                throw new NuaParseException("Require '(' after 'function' keyword while parsing function");

            List<string> parameterNames = new();
            if (TokenMatch(tokens, ref cursor, TokenKind.Identifier, out var firstParameterName))
            {
                parameterNames.Add(firstParameterName.Value!);

                while (TokenMatch(tokens, ref cursor, TokenKind.OptComma, out _))
                {
                    if (!TokenMatch(tokens, ref cursor, TokenKind.Identifier, out var anotherParameterName))
                        throw new NuaParseException("Require parameter name after ',' token while parsing function");

                    parameterNames.Add(anotherParameterName.Value!);
                }
            }

            if (!TokenMatch(tokens, ref cursor, TokenKind.ParenthesesRight, out _))
            {
                if (parameterNames.Count != 0)
                    throw new NuaParseException("Require ')' after parameters while parsing function");
                else
                    throw new NuaParseException("Require parameters after '(' token while parsing function");
            }

            if (!TokenMatch(tokens, ref cursor, TokenKind.BigBracketLeft, out _))
                throw new NuaParseException("Require '{' after ')' while parsing function");

            MultiExpr.Match(tokens, ref cursor, out var body);

            if (!TokenMatch(tokens, ref cursor, TokenKind.BigBracketRight, out _))
                throw new NuaParseException("Require '}' after function body while parsing function");

            index = cursor;
            expr = new FuncExpr(parameterNames, body);
            return true;
        }
    }
}
