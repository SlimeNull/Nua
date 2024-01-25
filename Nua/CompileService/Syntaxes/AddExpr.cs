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

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out AddExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!Expr.Match(ExprLevel.Mul, tokens, ref cursor, out var left))
                return false;
            if (!AddTailExpr.Match(tokens, ref cursor, out var tail))
                return false;

            index = cursor;
            expr = new AddExpr(left, tail);
            return true;
        }
    }
}
