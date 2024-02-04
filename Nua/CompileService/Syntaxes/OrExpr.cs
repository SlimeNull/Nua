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

        public override NuaValue? Evaluate(NuaContext context)
        {
            var leftValue = Left.Evaluate(context);

            if (leftValue == null)
                return Tail.Evaluate(context);

            if (leftValue is not NuaBoolean leftBoolean)
                return leftValue;

            if (!leftBoolean.Value)
                return Tail.Evaluate(context);

            return new NuaBoolean(true);
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
expr = null;
            int cursor = index;

            if (!AndExpr.Match(tokens, required, ref cursor, out parseStatus, out var left))
                return false;
            if (!OrTailExpr.Match(tokens, false, ref cursor, out var tailParseStatus, out var tail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = tail != null ? new OrExpr(left, tail) : left;
            return true;
        }
    }
}
