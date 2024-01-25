using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class MulExpr : Expr
    {
        public MulExpr(Expr left, MulTailExpr tail)
        {
            Left = left;
            Tail = tail;
        }

        public Expr Left { get; }
        public MulTailExpr Tail { get; }

        public override NuaValue? Eval(NuaContext context)
        {
            return Tail.Eval(context, Left);
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out MulExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!Expr.Match(ExprLevel.Process, tokens, ref cursor, out var left))
                return false;
            if (!MulTailExpr.Match(tokens, ref cursor, out var tail))
                return false;

            index = cursor;
            expr = new MulExpr(left, tail);
            return true;
        }
    }
}
