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
                if (intIndex < 0)
                    intIndex = list.Storage.Count + intIndex;
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
                if (intIndex < 0)
                    intIndex = str.Value.Length + intIndex;
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

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ValueIndexAccessTailExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.SquareBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Message = null;
                return false;
            }

            if (!Expr.Match(tokens, required, ref cursor, out parseStatus, out var indexExpr))
            {
                parseStatus.Intercept = true;
                if (parseStatus.Message == null)
                    parseStatus.Message = "Require index after '[' while parsing 'value-access-expression'";

                return false;
            }

            if (!TokenMatch(tokens, required, TokenKind.SquareBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Message = "Require ']' after index while parsing 'value-access-expression'";
                return false;
            }

            if (!ValueAccessTailExpr.Match(tokens, false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = new ValueIndexAccessTailExpr(indexExpr, nextTail);
            return true;
        }
    }
}
