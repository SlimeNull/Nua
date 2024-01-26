using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class OrExpr : Expr
    {
        public OrExpr(Expr left, OrTailExpr tail)
        {
            Left = left;
            Tail = tail;
        }

        public Expr Left { get; }
        public OrTailExpr Tail { get; }

        public override NuaValue? Eval(NuaContext context)
        {
            var leftValue = Left.Eval(context);

            if (leftValue == null)
                return Tail.Eval(context);

            if (leftValue is not NuaBoolean leftBoolean)
                return leftValue;

            if (!leftBoolean.Value)
                return Tail.Eval(context);

            return new NuaBoolean(true);
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!AndExpr.Match(tokens, ref cursor, out var left))
                return false;
            OrTailExpr.Match(tokens, ref cursor, out var tail);

            index = cursor;
            expr = tail != null ? new OrExpr(left, tail) : left;
            return true;
        }
    }
}
