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

        public override NuaValue? Evaluate(NuaContext context)
        {
            var leftValue = Left.Evaluate(context);

            if (leftValue is NuaBoolean leftBoolean && leftBoolean.Value)
                return Tail.Evaluate(context);

            return new NuaBoolean(false);
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
expr = null;
            int cursor = index;

            if (!EqualExpr.Match(tokens, required, ref cursor, out parseStatus, out var left))
                return false;
            if (AndTailExpr.Match(tokens, false, ref cursor, out var tailParseStatus, out var tail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = tail != null ? new AndExpr(left, tail) : left;
            return true;
        }
    }
}
