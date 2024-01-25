using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class EqualExpr : Expr
    {
        public EqualExpr(Expr left, EqualTailExpr tail)
        {
            Left = left;
            Tail = tail;
        }

        public Expr Left { get; }
        public EqualTailExpr Tail { get; }

        public override NuaValue? Eval(NuaContext context) => Tail.Eval(context, Left);

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out EqualExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!Expr.Match(ExprLevel.Compare, tokens, ref cursor, out var left))
                return false;
            if (!EqualTailExpr.Match(tokens, ref cursor, out var tail))
                return false;

            index = cursor;
            expr = new EqualExpr(left, tail);
            return true;
        }
    }
}
