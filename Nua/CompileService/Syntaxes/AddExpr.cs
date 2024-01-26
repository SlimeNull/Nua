using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class AddExpr : Expr
    {
        public AddExpr(Expr left, AddTailExpr tail)
        {
            Left = left;
            Tail = tail;
        }

        public Expr Left { get; }
        public AddTailExpr Tail { get; }

        public override NuaValue? Eval(NuaContext context)
        {
            return Tail.Eval(context, Left);
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!MulExpr.Match(tokens, ref cursor, out var left))
                return false;
            AddTailExpr.Match(tokens, ref cursor, out var tail);

            index = cursor;
            expr = tail != null ? new AddExpr(left, tail) : left;
            return true;
        }
    }
}
