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

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!AndExpr.Match(tokens, required, ref cursor, out requireMoreTokens, out message, out var left))
                return false;
            if (!OrTailExpr.Match(tokens, false, ref cursor, out var tailRequireMoreTokens, out var tailMessage, out var tail) && tailRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = tailMessage;
                return false;
            }

            index = cursor;
            expr = tail != null ? new OrExpr(left, tail) : left;
            return true;
        }
    }
}
