using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes
{
    public abstract class ValueExpr : Expr
    {
        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            return
                FuncExpr.Match(tokens, ref index, out expr) ||
                DictExpr.Match(tokens, ref index, out expr) ||
                ListExpr.Match(tokens, ref index, out expr) ||
                QuotedExpr.Match(tokens, ref index, out expr) ||
                VariableExpr.Match(tokens, ref index, out expr) ||
                ConstExpr.Match(tokens, ref index, out expr);
        }
    }
}
