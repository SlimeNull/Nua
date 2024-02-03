using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class MulExpr : Expr
    {
        public MulExpr(Expr left, MulTailExpr tail)
        {
            Left = left;
            Tail = tail;
        }

        public Expr Left { get; }
        public MulTailExpr Tail { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            return Tail.Evaluate(context, Left);
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!ProcessExpr.Match(tokens, required, ref cursor, out requireMoreTokens, out message, out var left))
                return false;
            if (!MulTailExpr.Match(tokens, false, ref cursor, out var tailRequireMoreTokens, out var tailMessage, out var tail) && tailRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = tailMessage;
                return false;
            }

            index = cursor;
            expr = tail != null ? new MulExpr(left, tail) : left;
            return true;
        }
    }
}
