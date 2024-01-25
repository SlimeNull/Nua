using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AssignTailExpr : Expr
    {
        public AssignTailExpr(Expr right, AssignTailExpr? nextTail)
        {
            Right = right;
            NextTail = nextTail;
        }

        public Expr Right { get; }
        public AssignTailExpr? NextTail { get; }

        public override NuaValue? Eval(NuaContext context)
        {
            if (NextTail == null)
                return Right.Eval(context);

            if (Right is ValueAccessExpr valueAccessExpr)
            {
                var value = NextTail.Eval(context);
                valueAccessExpr.SetMemberValue(context, value);

                return value;
            }
            else if (Right is VariableExpr variableExpr)
            {
                var value = NextTail.Eval(context);
                variableExpr.SetValue(context, value);

                return value;
            }
            else
            {
                throw new NuaEvalException("Only Value member or Variable can be assigned");
            }
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out AssignTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.OptAssign)
                return false;
            cursor++;

            if (!Expr.Match(ExprLevel.Or, tokens, ref cursor, out var right))
                return false;

            Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new AssignTailExpr(right, nextTail);
            return true;
        }
    }
}
