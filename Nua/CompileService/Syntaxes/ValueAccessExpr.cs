using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ValueAccessExpr : PrimaryExpr
    {
        public ValueAccessExpr(VariableExpr variable, ValueAccessTailExpr tail)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
            Tail = tail ?? throw new ArgumentNullException(nameof(tail));
        }

        public VariableExpr Variable { get; }
        public ValueAccessTailExpr Tail { get; }

        public override NuaValue? Eval(NuaContext context) => Tail.Eval(context, Variable);
        public void SetMemberValue(NuaContext context, NuaValue? value) => Tail.SetMemberValue(context, Variable, value);
        public void SetMemberValue(NuaContext context, Expr valueExpr) => Tail.SetMemberValue(context, Variable, valueExpr);

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ValueAccessExpr? expr)
        {
            expr = null;
            if (index < 0 || index >= tokens.Count)
                return false;

            int cursor = index;

            if (!VariableExpr.Match(tokens, ref cursor, out var variable))
                return false;
            if (!ValueAccessTailExpr.Match(tokens, ref cursor, out var tail))
                return false;

            expr = new ValueAccessExpr(variable, tail);
            index = cursor;
            return true;
        }
    } 
}
