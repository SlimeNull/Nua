using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Nua.CompileService.Syntaxes
{

    public abstract class Expr : Syntax
    {
        public static bool Match(ExprLevel level, IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            if (level.HasFlag(ExprLevel.Assign) && AssignExpr.Match(tokens, ref index, out var expr11))
                expr = expr11;
            else if (level.HasFlag(ExprLevel.Or) && OrExpr.Match(tokens, ref index, out var expr10))
                expr = expr10;
            else if (level.HasFlag(ExprLevel.And) && AndExpr.Match(tokens, ref index, out var expr9))
                expr = expr9;
            else if (level.HasFlag(ExprLevel.Equal) && EqualExpr.Match(tokens, ref index, out var expr8))
                expr = expr8;
            else if (level.HasFlag(ExprLevel.Compare) && CompareExpr.Match(tokens, ref index, out var expr7))
                expr = expr7;
            else if (level.HasFlag(ExprLevel.Add) && AddExpr.Match(tokens, ref index, out var expr6))
                expr = expr6;
            else if (level.HasFlag(ExprLevel.Mul) && MulExpr.Match(tokens, ref index, out var expr5))
                expr = expr5;
            else if (level.HasFlag(ExprLevel.Process) && ProcessExpr.Match(tokens, ref index, out var expr4))
                expr = expr4;
            else if (level.HasFlag(ExprLevel.Unary) && UnaryExpr.Match(tokens, ref index, out var expr3))
                expr = expr3;
            else if (level.HasFlag(ExprLevel.Primary) && PrimaryExpr.Match(tokens, ref index, out var expr2))
                expr = expr2;
            else if (level.HasFlag(ExprLevel.Value) && ValueExpr.Match(tokens, ref index, out var expr1))
                expr = expr1;
            else
                return false;

            return true;
        }
    }
}
