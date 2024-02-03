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

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!EqualExpr.Match(tokens, required, ref cursor, out requireMoreTokens, out message, out var left))
                return false;
            if (AndTailExpr.Match(tokens, false, ref cursor, out var tailRequireMoreTokens, out var tailMessage, out var tail) && tailRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = tailMessage;
                return false;
            }

            index = cursor;
            expr = tail != null ? new AndExpr(left, tail) : left;
            return true;
        }
    }
}
