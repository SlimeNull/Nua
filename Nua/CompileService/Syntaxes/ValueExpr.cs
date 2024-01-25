using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes
{
    public abstract class ValueExpr : Expr
    {
        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ValueExpr? expr)
        {
            expr = null;
            if (DictExpr.Match(tokens, ref index, out var expr5))
                expr = expr5;
            else if (ListExpr.Match(tokens, ref index, out var expr4))
                expr = expr4;
            else if (QuotedExpr.Match(tokens, ref index, out var expr3))
                expr = expr3;
            else if (VariableExpr.Match(tokens, ref index, out var expr2))
                expr = expr2;
            else if (ConstExpr.Match(tokens, ref index, out var expr1))
                expr = expr1;
            else
                return false;

            return true;
        }
    }
}
