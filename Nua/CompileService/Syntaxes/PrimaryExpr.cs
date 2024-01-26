using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes
{

    public abstract class PrimaryExpr : Expr
    {
        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            return
                ValueAccessExpr.Match(tokens, ref index, out expr) ||
                SuffixSelfAddExpr.Match(tokens, ref index, out expr) ||
                ValueExpr.Match(tokens, ref index, out expr);
        }
    }
}
