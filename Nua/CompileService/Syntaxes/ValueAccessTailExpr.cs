using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public abstract class ValueAccessTailExpr : Expr
    {
        public ValueAccessTailExpr? NextTail { get; }

        public ValueAccessTailExpr(ValueAccessTailExpr? nextTail)
        {
            NextTail = nextTail;
        }

        public NuaValue? Evaluate(NuaContext context, Expr expr)
        {
            var valueToAccess = expr.Evaluate(context);

            if (valueToAccess == null)
                throw new NuaEvalException("Unable to access member of null value");

            return Evaluate(context, valueToAccess);
        }

        public void SetMemberValue(NuaContext context, NuaValue valueToAccess, NuaValue? newMemberValue)
        {
            if (valueToAccess is not NuaTable table)
                throw new NuaEvalException("Unable to access member of non-table value");

            if (NextTail != null)
            {
                var value = Evaluate(context);

                if (value == null)
                    throw new NuaEvalException("Unable to access member of null value");

                NextTail.SetMemberValue(context, value, newMemberValue);
            }
            else
            {
                NuaValue? key;
                if (this is ValueMemberAccessTailExpr memberAccessTail)
                    key = new NuaString(memberAccessTail.Name);
                else if (this is ValueIndexAccessTailExpr indexAccessTail)
                    key = indexAccessTail.Index.Evaluate(context);
                else
                    throw new NuaEvalException("Only Value member or Variable can be assigned");

                if (key != null)
                    table.Set(key, newMemberValue);
            }
        }

        public void SetMemberValue(NuaContext context, Expr expr, NuaValue? newMemberValue)
        {
            var valueToAccess = expr.Evaluate(context);

            if (valueToAccess == null)
                throw new NuaEvalException("Unable to access member of null value");

            SetMemberValue(context, valueToAccess, newMemberValue);
        }

        public void SetMemberValue(NuaContext context, Expr expr, Expr newMemberValueExpr)
        {

            var newMemberValue = newMemberValueExpr.Evaluate(context);

            SetMemberValue(context, expr, newMemberValue);
        }

        public abstract NuaValue? Evaluate(NuaContext context, NuaValue? valueToAccess);
        public override NuaValue? Evaluate(NuaContext context) => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out ValueAccessTailExpr? expr)
        {
            expr = null;
            if (ValueIndexAccessTailExpr.Match(tokens, required, ref index, out requireMoreTokens, out message, out var expr3))
                expr = expr3;
            else if (ValueInvokeAccessTailExpr.Match(tokens, required, ref index, out requireMoreTokens, out message, out var expr2))
                expr = expr2;
            else if (ValueMemberAccessTailExpr.Match(tokens, required, ref index, out requireMoreTokens, out message, out var expr1))
                expr = expr1;
            else
                return false;

            return true;
        }
    }
}
