using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ValueAccessExpr : PrimaryExpr
    {
        public ValueAccessExpr(ValueExpr valueExpr, ValueAccessTailExpr tailExpr)
        {
            ValueExpr = valueExpr ?? throw new ArgumentNullException(nameof(valueExpr));
            TailExpr = tailExpr;
        }

        public ValueExpr ValueExpr { get; }
        public ValueAccessTailExpr TailExpr { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            return TailExpr.Evaluate(context, ValueExpr);
        }

        public void SetMemberValue(NuaContext context, NuaValue? value)
        {
            TailExpr.SetMemberValue(context, ValueExpr, value);
        }

        public void SetMemberValue(NuaContext context, Expr valueExpr)
        {
            TailExpr.SetMemberValue(context, ValueExpr, valueExpr.Evaluate(context));
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!ValueExpr.Match(tokens, required, ref cursor, out parseStatus, out var variable))
                return false;

            if (!ValueAccessTailExpr.Match(tokens, false, ref cursor, out var tailParseStatus, out var tail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            expr = tail != null ? new ValueAccessExpr((ValueExpr)variable, tail) : (ValueExpr)variable;
            index = cursor;
            return true;
        }
    }
}
