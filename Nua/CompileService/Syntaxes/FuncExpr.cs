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

        public override NuaValue? Evaluate(NuaContext context)
            => new NuaNativeFunction(Body, ParameterNames.ToArray());

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;
            if (!TokenMatch(tokens, required, TokenKind.KwdFunction, ref cursor, out _, out _))
            {
                requireMoreTokens = false;
                message = null;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.ParenthesesLeft, ref cursor, out requireMoreTokens, out _))
            {
                message = "Require '(' after 'function' keyword while parsing function";
                return false;
            }

            List<string> parameterNames = new();
            if (TokenMatch(tokens, false, TokenKind.Identifier, ref cursor, out _, out var firstParameterName))
            {
                parameterNames.Add(firstParameterName.Value!);

                while (TokenMatch(tokens, false, TokenKind.OptComma, ref cursor, out _, out _))
                {
                    if (!TokenMatch(tokens, true, TokenKind.Identifier, ref cursor, out requireMoreTokens, out var anotherParameterName))
                    {
                        message = "Require parameter name after ',' token while parsing function";
                        return false;
                    }

                    parameterNames.Add(anotherParameterName.Value!);
                }
            }

            if (!TokenMatch(tokens, true, TokenKind.ParenthesesRight, ref cursor, out requireMoreTokens, out _))
            {
                if (parameterNames.Count != 0)
                    message = "Require ')' after parameters while parsing function";
                else
                    message = "Require parameters after '(' token while parsing function";

                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out requireMoreTokens, out _))
            {
                message = "Require '{' after ')' while parsing function";
                return false;
            }

            if (!MultiExpr.Match(tokens, false, ref cursor, out var bodyRequireMoreTokens, out var bodyMessage, out var body) && bodyRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = bodyMessage;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out requireMoreTokens, out _))
            {
                message = "Require '}' after function body while parsing function";
                return false;
            }

            index = cursor;
            expr = new FuncExpr(parameterNames, body);
            message = null;
            return true;
        }
    }
}
