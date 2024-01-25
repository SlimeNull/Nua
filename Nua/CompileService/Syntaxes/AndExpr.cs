using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AndExpr : Expr
    {
        public AndExpr(Expr left, AndTailExpr tail)
        {
            Left = left;
            Tail = tail;
        }

        public Expr Left { get; }
        public AndTailExpr Tail { get; }

        public override NuaValue? Eval(NuaContext context)
        {
            var leftValue = Left.Eval(context);

            if (leftValue is NuaBoolean leftBoolean && leftBoolean.Value)
                return Tail.Eval(context);

            return new NuaBoolean(false);
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out AndExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!Expr.Match(ExprLevel.Equal, tokens, ref cursor, out var left))
                return false;
            if (!AndTailExpr.Match(tokens, ref cursor, out var tail))
                return false;

            index = cursor;
            expr = new AndExpr(left, tail);
            return true;
        }
    }
}
