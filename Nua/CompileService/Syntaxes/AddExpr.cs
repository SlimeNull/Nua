using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class AddExpr : Expr
    {
        public AddExpr(Expr leftExpr, AddTailExpr tailExpr)
        {
            LeftExpr = leftExpr;
            TailExpr = tailExpr;
        }

        public Expr LeftExpr { get; }
        public AddTailExpr TailExpr { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            return TailExpr.Evaluate(context, LeftExpr);
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!MulExpr.Match(tokens, required, ref cursor, out parseStatus, out var left))
                return false;
            if (!AddTailExpr.Match(tokens, false, ref cursor, out var tailParseStatus, out var tail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = tail != null ? new AddExpr(left, tail) : left;
            return true;
        }
    }
}
