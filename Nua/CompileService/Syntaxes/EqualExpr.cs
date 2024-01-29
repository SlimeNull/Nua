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

        public override NuaValue? Evaluate(NuaContext context)
        {
            return Tail.Evaluate(context, Left);
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!CompareExpr.Match(tokens, ref cursor, out var left))
                return false;
            EqualTailExpr.Match(tokens, ref cursor, out var tail);

            index = cursor;
            expr = tail != null ? new EqualExpr(left, tail) : left;
            return true;
        }
    }
}
