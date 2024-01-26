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

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!ProcessExpr.Match(tokens, ref cursor, out var left))
                return false;
            MulTailExpr.Match(tokens, ref cursor, out var tail);

            index = cursor;
            expr = tail != null ? new MulExpr(left, tail) : left;
            return true;
        }
    }
}
