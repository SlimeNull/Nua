using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes
{
    public abstract class UnaryExpr : Expr
    {
        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out UnaryExpr? expr)
        {
            expr = null;

            if (PrefixSelfAddExpr.Match(tokens, ref index, out var expr2))
                expr = expr2;
            else if (InvertNumberExpr.Match(tokens, ref index, out var expr1))
                expr = expr1;
            else
                return false;

            return true;
        }
    }
}
