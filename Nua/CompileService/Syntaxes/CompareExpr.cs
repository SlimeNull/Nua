using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class CompareExpr : Expr
    {
        public CompareExpr(Expr left, CompareTailExpr tail)
        {
            Left = left;
            Tail = tail;
        }

        public Expr Left { get; }
        public CompareTailExpr Tail { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            return Tail.Evaluate(context, Left);
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
expr = null;
            int cursor = index;

            if (!AddExpr.Match(tokens, required, ref cursor, out parseStatus, out var left))
                return false;
            if (!CompareTailExpr.Match(tokens, false, ref cursor, out var tailParseStatus, out var tail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = tail != null ? new CompareExpr(left, tail) : left;
            return true;
        }
    }
}
