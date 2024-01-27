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

        public override NuaValue? Eval(NuaContext context, NuaValue? valueToAccess)
        {
            if (valueToAccess == null)
                throw new NuaEvalException("Unable to index a null value");
            
            NuaValue? result;
            if (valueToAccess is NuaTable table)
            {
                NuaValue? index = Index.Eval(context);

                if (index == null)
                    throw new NuaEvalException("Index is null");

                result = table.Get(index);
            }
            else if (valueToAccess is NuaList list)
            {
                NuaValue? index = Index.Eval(context);

                if (index == null)
                    throw new NuaEvalException("Index is null");
                if (index is not NuaNumber number)
                    throw new NuaEvalException("Unable to index a List with non-number value");

                int intIndex = (int)number.Value;
                if (intIndex != number.Value)
                    result = null;
                if (intIndex < 0 || intIndex >= list.Storage.Count)
                    result = null;

                result = list.Storage[intIndex];
            }
            else if (valueToAccess is NuaString str)
            {
                NuaValue? index = Index.Eval(context);

                if (index == null)
                    throw new NuaEvalException("Index is null");
                if (index is not NuaNumber number)
                    throw new NuaEvalException("Unable to index a List with non-number value");

                int intIndex = (int)number.Value;
                if (intIndex != number.Value)
                    result = null;
                if (intIndex < 0 || intIndex >= str.Value.Length)
                    result = null;

                result = new NuaString(str.Value.Substring(intIndex, 1));
            }
            else
            {
                throw new NuaEvalException("Only Dictionary, List and String can be indexed");
            }

            if (NextTail != null)
                result = NextTail.Eval(context, result);

            return result;
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ValueIndexAccessTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, ref cursor, TokenKind.SquareBracketLeft, out _))
                return false;

            if (!Expr.MatchAny(tokens, ref cursor, out var indexExpr))
                throw new NuaParseException("Require index after '[' while parsing 'value-access-expression'");

            if (!TokenMatch(tokens, ref cursor, TokenKind.SquareBracketRight, out _))
                throw new NuaParseException("Require ']' after index while parsing 'value-access-expression'");

            ValueAccessTailExpr.Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new ValueIndexAccessTailExpr(indexExpr, nextTail);
            return true;
        }
    }
}
