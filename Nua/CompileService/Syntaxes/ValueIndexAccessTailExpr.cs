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

        public override NuaValue? Evaluate(NuaContext context, NuaValue? valueToAccess)
        {
            if (valueToAccess == null)
                throw new NuaEvalException("Unable to index a null value");

            NuaValue? result;
            if (valueToAccess is NuaTable table)
            {
                NuaValue? index = Index.Evaluate(context);

                if (index == null)
                    throw new NuaEvalException("Index is null");

                result = table.Get(index);
            }
            else if (valueToAccess is NuaList list)
            {
                NuaValue? index = Index.Evaluate(context);

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
                NuaValue? index = Index.Evaluate(context);

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
                result = NextTail.Evaluate(context, result);

            return result;
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out ValueIndexAccessTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.SquareBracketLeft, ref cursor, out requireMoreTokens, out _))
            {
                message = null;
                return false;
            }

            if (!Expr.MatchAny(tokens, required, ref cursor, out _, out message, out var indexExpr))
            {
                requireMoreTokens = true;
                if (message == null)
                    message = "Require index after '[' while parsing 'value-access-expression'";

                return false;
            }

            if (!TokenMatch(tokens, required, TokenKind.SquareBracketRight, ref cursor, out requireMoreTokens, out _))
            {
                message = "Require ']' after index while parsing 'value-access-expression'";
                return false;
            }

            if (!ValueAccessTailExpr.Match(tokens, false, ref cursor, out var tailRequireMoreTokens, out var tailMessage, out var nextTail) && tailRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = tailMessage;
                return false;
            }

            index = cursor;
            expr = new ValueIndexAccessTailExpr(indexExpr, nextTail);
            return true;
        }
    }
}
