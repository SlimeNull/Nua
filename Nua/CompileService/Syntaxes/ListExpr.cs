using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ListExpr : ValueExpr
    {
        public IEnumerable<Expr> Values { get; }

        public ListExpr(IEnumerable<Expr> values)
        {
            Values = values;
        }

        public override NuaValue? Evaluate(NuaContext context)
        {
            NuaList list = new();
            foreach (var value in Values)
                list.Storage.Add(value.Evaluate(context));

            return list;
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.SquareBracketLeft, ref cursor, out requireMoreTokens, out _))
            {
                message = null;
                return false;
            }

            if (!ChainExpr.Match(tokens, false, ref cursor, out var chainRequireMoreTokens, out var chainMessage, out var chain) && chainRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = chainMessage;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.SquareBracketRight, ref cursor, out requireMoreTokens, out _))
            {
                if (chain != null)
                    message = "Expect ']' after '[' while parsing 'list-expression'";
                else
                    message = "Expect expression after '[' while parsing 'list-expression'";

                return false;
            }

            index = cursor;
            expr = new ListExpr(chain?.Expressions ?? []);
            message = null;
            return true;
        }

    }
}
