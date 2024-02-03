using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes
{
    public abstract class ValueExpr : Expr
    {
        static readonly Matcher[] matchers = new Matcher[]
        {
            FuncExpr.Match,
            TableExpr.Match,
            ListExpr.Match,
            QuotedExpr.Match,
            VariableExpr.Match,
            ConstExpr.Match,
        };

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            requireMoreTokens = required;
            message = null;
            expr = null;

            for (int i = 0; i < matchers.Length; i++)
            {
                var matcher = matchers[i];
                bool isLast = i == matchers.Length - 1;

                if (matcher.Invoke(tokens, isLast ? required : false, ref index, out requireMoreTokens, out message, out expr))
                    return true;
                else if (requireMoreTokens)
                    return false;
            }

            return false;
        }
    }
}
