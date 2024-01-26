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

        public override NuaValue? Eval(NuaContext context)
        {
            NuaList list = new();
            foreach (var value in Values)
                list.Storage.Add(value.Eval(context));

            return list;
        }

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, ref cursor, TokenKind.SquareBracketLeft, out _))
                return false;

            ChainExpr.Match(tokens, ref cursor, out var chain);

            if (!TokenMatch(tokens, ref cursor, TokenKind.SquareBracketRight, out _))
            {
                if (chain != null)
                    throw new NuaParseException("Expect ']' after '[' while parsing 'list-expression'");
                else
                    throw new NuaParseException("Expect expression after '[' while parsing 'list-expression'");
            }

            index = cursor;
            expr = new ListExpr(chain?.Expressions ?? []);
            return true;
        }

    }
}
