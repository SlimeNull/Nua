using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AssignExpr : Expr
    {
        public AssignExpr(Expr left, AssignTailExpr tail)
        {
            Left = left;
            Tail = tail;
        }

        public Expr Left { get; }
        public AssignTailExpr Tail { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            return Tail.Evaluate(context, Left);
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!PrimaryExpr.Match(tokens, required, ref cursor, out parseStatus, out var left))
                return false;
            if (!AssignTailExpr.Match(tokens, required, ref cursor, out parseStatus, out var tail))
                return false;

            index = cursor;
            expr = new AssignExpr(left, tail);
            return true;
        }
    }
}
