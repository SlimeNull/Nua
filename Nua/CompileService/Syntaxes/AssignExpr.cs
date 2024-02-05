using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AssignExpr : Expr
    {
        public AssignExpr(Expr leftExpr, AssignTailExpr tailExpr)
        {
            LeftExpr = leftExpr;
            TailExpr = tailExpr;
        }

        public Expr LeftExpr { get; }
        public AssignTailExpr TailExpr { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            return TailExpr.Evaluate(context, LeftExpr);
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!OrExpr.Match(tokens, required, ref cursor, out parseStatus, out var left))
                return false;
            if (!AssignTailExpr.Match(tokens, false, ref cursor, out parseStatus, out var tail) && parseStatus.Intercept)
                return false;

            index = cursor;
            expr = tail != null ? new AssignExpr(left, tail) : left;
            return true;
        }
    }
}
