using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ValueIndexAccessTailExpr : ValueAccessTailExpr
    {
        public ValueIndexAccessTailExpr(Expr index, ValueAccessTailExpr? nextTail) : base(nextTail)
        {
            Index = index;
        }

        public Expr Index { get; }

        public override NuaValue? Eval(NuaContext context, NuaValue valueToAccess)
        {
            NuaValue? valueToIndex = valueToAccess;

            if (valueToIndex == null)
                throw new NuaEvalException("Unable to index a null value");

            if (valueToIndex is NuaDictionary dict)
            {
                NuaValue? index = Index.Eval(context);

                if (index == null)
                    throw new NuaEvalException("Index is null");

                return dict.Get(index);
            }
            else if (valueToIndex is NuaList list)
            {
                NuaValue? index = Index.Eval(context);

                if (index == null)
                    throw new NuaEvalException("Index is null");
                if (index is not NuaNumber number)
                    throw new NuaEvalException("Unable to index a List with non-number value");

                int intIndex = (int)number.Value;
                if (intIndex != number.Value)
                    return null;
                if (intIndex < 0 || intIndex >= list.Storage.Count)
                    return null;

                return list.Storage[intIndex];
            }
            else if (valueToIndex is NuaString str)
            {
                NuaValue? index = Index.Eval(context);

                if (index == null)
                    throw new NuaEvalException("Index is null");
                if (index is not NuaNumber number)
                    throw new NuaEvalException("Unable to index a List with non-number value");

                int intIndex = (int)number.Value;
                if (intIndex != number.Value)
                    return null;
                if (intIndex < 0 || intIndex >= str.Value.Length)
                    return null;

                return new NuaString(str.Value.Substring(intIndex, 1));
            }
            else
            {
                throw new NuaEvalException("Only Dictionary, List and String can be indexed");
            }
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ValueIndexAccessTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.SquareBracketLeft)
                return false;
            cursor++;

            if (!Expr.Match(ExprLevel.All, tokens, ref cursor, out var indexExpr))
                return false;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.SquareBracketRight)
                return false;
            cursor++;

            ValueAccessTailExpr.Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new ValueIndexAccessTailExpr(indexExpr, nextTail);
            return true;
        }
    }
}
