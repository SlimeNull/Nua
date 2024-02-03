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

        public override NuaValue? Evaluate(NuaContext context)
        {
            return Tail.Evaluate(context, Left);
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!MulExpr.Match(tokens, required, ref cursor, out requireMoreTokens, out message, out var left))
                return false;
            if (!AddTailExpr.Match(tokens, false, ref cursor, out var tailRequireMoreTokens, out string? tailMessage, out var tail) && tailRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = tailMessage;
                return false;
            }

            index = cursor;
            expr = tail != null ? new AddExpr(left, tail) : left;
            return true;
        }
    }
}
