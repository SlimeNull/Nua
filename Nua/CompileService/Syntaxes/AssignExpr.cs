using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AssignExpr : Expr
    {
        public AssignExpr(Expr left, AssignTailExpr tail)
        {
            Left = left;
            Tail = tail;
        }

        public Expr Left { get; }
        public AssignTailExpr Tail { get; }

        public override NuaValue? Eval(NuaContext context)
        {
            if (Left is ValueAccessExpr valueAccessExpr)
            {
                var value = Tail.Eval(context);
                valueAccessExpr.SetMemberValue(context, value);

                return value;
            }
            else if (Left is VariableExpr variableExpr)
            {
                var value = Tail.Eval(context);
                variableExpr.SetValue(context, value);

                return value;
            }
            else
            {
                throw new NuaEvalException("Only Value member or Variable can be assigned");
            }
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!PrimaryExpr.Match(tokens, ref cursor, out var left))
                return false;
            if (!AssignTailExpr.Match(tokens, ref cursor, out var tail))
                return false;

            index = cursor;
            expr = tail != null ? new AssignExpr(left, tail) : left;
            return true;
        }
    }
}
