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

        public override NuaValue? Eval(NuaContext context)
        {
            return Tail.Eval(context, Value);
        }

        public void SetMemberValue(NuaContext context, NuaValue? value)
        {
            Tail.SetMemberValue(context, Value, value);
        }

        public void SetMemberValue(NuaContext context, Expr valueExpr)
        {
            Tail.SetMemberValue(context, Value, valueExpr.Eval(context));
        }

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            if (index < 0 || index >= tokens.Count)
                return false;

            int cursor = index;

            if (!ValueExpr.Match(tokens, ref cursor, out var variable))
                return false;
            ValueAccessTailExpr.Match(tokens, ref cursor, out var tail);

            expr = tail != null ? new ValueAccessExpr((ValueExpr)variable, tail) : (ValueExpr)variable;
            index = cursor;
            return true;
        }
    } 
}
