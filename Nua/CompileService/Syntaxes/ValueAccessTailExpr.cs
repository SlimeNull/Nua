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

        public NuaValue? Eval(NuaContext context, Expr expr)
        {
            var valueToAccess = expr.Eval(context);

            if (valueToAccess == null)
                throw new NuaEvalException("Unable to access member of null value");

            return Eval(context, valueToAccess);
        }

        public void SetMemberValue(NuaContext context, NuaValue valueToAccess, NuaValue? newMemberValue)
        {
            if (valueToAccess is not NuaDictionary dict)
                throw new NuaEvalException("Unable to access member of non-dictionary value");

            if (NextTail != null)
            {
                var value = Eval(context);

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
                    key = indexAccessTail.Index.Eval(context);
                else
                    throw new NuaEvalException("Only Value member or Variable can be assigned");

                if (key != null)
                    dict.Set(key, newMemberValue);
            }
        }

        public void SetMemberValue(NuaContext context, Expr expr, NuaValue? newMemberValue)
        {
            var valueToAccess = expr.Eval(context);

            if (valueToAccess == null)
                throw new NuaEvalException("Unable to access member of null value");

            SetMemberValue(context, valueToAccess, newMemberValue);
        }

        public void SetMemberValue(NuaContext context, Expr expr, Expr newMemberValueExpr)
        {

            var newMemberValue = newMemberValueExpr.Eval(context);

            SetMemberValue(context, expr, newMemberValue);
        }

        public abstract NuaValue? Eval(NuaContext context, NuaValue valueToAccess);
        public override NuaValue? Eval(NuaContext context) => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ValueAccessTailExpr? expr)
        {
            expr = null;
            if (ValueIndexAccessTailExpr.Match(tokens, ref index, out var expr3))
                expr = expr3;
            else if (ValueInvokeAccessTailExpr.Match(tokens, ref index, out var expr2))
                expr = expr2;
            else if (ValueMemberAccessTailExpr.Match(tokens, ref index, out var expr1))
                expr = expr1;
            else
                return false;

            return true;
        }
    }
}
