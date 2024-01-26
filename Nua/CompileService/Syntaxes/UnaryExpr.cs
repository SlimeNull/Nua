using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes
{
    public abstract class UnaryExpr : Expr
    {
        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            return
                PrefixSelfAddExpr.Match(tokens, ref index, out expr) ||
                InvertNumberExpr.Match(tokens, ref index, out expr) ||
                PrimaryExpr.Match(tokens, ref index, out expr);
        }
    }
}
