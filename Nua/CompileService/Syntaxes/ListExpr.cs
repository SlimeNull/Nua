using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ListExpr : ValueExpr
    {
        public ListExpr(ChainExpr chain)
        {
            Chain = chain;
        }

        public ChainExpr Chain { get; }

        public override NuaValue? Eval(NuaContext context)
        {
            NuaList list = new();
            foreach (var value in Chain.Expressions)
                list.Storage.Add(value.Eval(context));

            return list;
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ListExpr? expr)
        {
            expr = null;
            if (index < 0 || index >= tokens.Count)
                return false;
            if (tokens[index].Kind != TokenKind.SquareBracketLeft)
                return false;

            int cursor = index;
            cursor++;

            if (!ChainExpr.Match(tokens, ref cursor, out var chain))
                return false;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.SquareBracketRight)
                return false;
            cursor++;

            index = cursor;
            expr = new ListExpr(chain);
            return true;
        }

    }
}
