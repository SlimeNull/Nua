using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class FuncExpr : ValueExpr
    {
        public IReadOnlyList<string> ParameterNames { get; }
        public MultiExpr? BodyExpr { get; }

        public FuncExpr(IEnumerable<string> parameterNames, MultiExpr? bodyExpr)
        {
            ParameterNames = parameterNames.ToList().AsReadOnly();
            BodyExpr = bodyExpr;
        }

        public override NuaValue? Evaluate(NuaContext context)
            => new NuaNativeFunction(BodyExpr, ParameterNames.ToArray());

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;
            if (!TokenMatch(tokens, required, TokenKind.KwdFunction, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = required;
                parseStatus.Message = null;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.ParenthesesLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require '(' after 'function' keyword while parsing function";
                return false;
            }

            List<string> parameterNames = new();
            if (TokenMatch(tokens, false, TokenKind.Identifier, ref cursor, out _, out var firstParameterName))
            {
                parameterNames.Add(firstParameterName.Value!);

                while (TokenMatch(tokens, false, TokenKind.OptComma, ref cursor, out _, out _))
                {
                    if (!TokenMatch(tokens, true, TokenKind.Identifier, ref cursor, out parseStatus.RequireMoreTokens, out var anotherParameterName))
                    {
                        parseStatus.Intercept = true;
                        parseStatus.Message = "Require parameter name after ',' token while parsing function";
                        return false;
                    }

                    parameterNames.Add(anotherParameterName.Value!);
                }
            }

            if (!TokenMatch(tokens, true, TokenKind.ParenthesesRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                if (parameterNames.Count != 0)
                    parseStatus.Message = "Require ')' after parameters while parsing function";
                else
                    parseStatus.Message = "Require parameters after '(' token while parsing function";

                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require '{' after ')' while parsing function";
                return false;
            }

            if (!MultiExpr.Match(tokens, false, ref cursor, out var bodyParseStatus, out var body) && bodyParseStatus.Intercept)
            {
                parseStatus = bodyParseStatus;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require '}' after function body while parsing function";
                return false;
            }

            index = cursor;
            expr = new FuncExpr(parameterNames, body);
            parseStatus.Message = null;
            return true;
        }
    }
}
