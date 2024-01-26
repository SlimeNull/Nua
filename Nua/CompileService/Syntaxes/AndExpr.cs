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

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!EqualExpr.Match(tokens, ref cursor, out var left))
                return false;
            AndTailExpr.Match(tokens, ref cursor, out var tail);

            index = cursor;
            expr = tail != null ? new AndExpr(left, tail) : left;
            return true;
        }
    }
}
