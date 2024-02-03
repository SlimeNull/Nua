using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ValueAccessExpr : PrimaryExpr
    {
        public ValueAccessExpr(ValueExpr value, ValueAccessTailExpr tail)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Tail = tail;
        }

        public ValueExpr Value { get; }
        public ValueAccessTailExpr Tail { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            return Tail.Evaluate(context, Value);
        }

        public void SetMemberValue(NuaContext context, NuaValue? value)
        {
            Tail.SetMemberValue(context, Value, value);
        }

        public void SetMemberValue(NuaContext context, Expr valueExpr)
        {
            Tail.SetMemberValue(context, Value, valueExpr.Evaluate(context));
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!ValueExpr.Match(tokens, required, ref cursor, out requireMoreTokens, out message, out var variable))
                return false;

            if (!ValueAccessTailExpr.Match(tokens, false, ref cursor, out var tailRequireMoreTokens, out var tailMessage, out var tail) && tailRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = tailMessage;
                return false;
            }

            expr = tail != null ? new ValueAccessExpr((ValueExpr)variable, tail) : (ValueExpr)variable;
            index = cursor;
            return true;
        }
    } 
}
