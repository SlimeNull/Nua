using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes
{

    public abstract class PrimaryExpr : Expr
    {
        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out PrimaryExpr? expr)
        {
            expr = null;
            if (ValueAccessExpr.Match(tokens, ref index, out var expr2))
                expr = expr2;
            else if (SuffixSelfAddExpr.Match(tokens, ref index, out var expr1))
                expr = expr1;
            else
                return false;

            return true;
        }
    }
}
