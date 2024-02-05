using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ListExpr : ValueExpr
    {
        public IEnumerable<Expr> ValueExpressions { get; }

        public ListExpr(IEnumerable<Expr> valueExpressions)
        {
            ValueExpressions = valueExpressions;
        }

        public override NuaValue? Evaluate(NuaContext context)
        {
            NuaList list = new();
            foreach (var value in ValueExpressions)
                list.Storage.Add(value.Evaluate(context));

            return list;
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.SquareBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Message = null;
                return false;
            }

            if (!ChainExpr.Match(tokens, false, ref cursor, out var chainParseStatus, out var chain) && chainParseStatus.Intercept)
            {
                parseStatus = chainParseStatus;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.SquareBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                if (chain != null)
                    parseStatus.Message = "Expect ']' after '[' while parsing 'list-expression'";
                else
                    parseStatus.Message = "Expect expression after '[' while parsing 'list-expression'";

                return false;
            }

            index = cursor;
            expr = new ListExpr(chain?.Expressions ?? []);
            parseStatus.Message = null;
            return true;
        }

    }
}
