using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes
{
    public abstract class UnaryExpr : Expr
    {
        static readonly Matcher[] matchers = new Matcher[]
        {
            PrefixSelfAddExpr.Match,
            InvertNumberExpr.Match,
            PrimaryExpr.Match,
        };

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus.RequireMoreTokens = required;
            parseStatus.Message = null;
            parseStatus = new();
expr = null;

            for (int i = 0; i < matchers.Length; i++)
            {
                var matcher = matchers[i];
                bool isLast = i == matchers.Length - 1;

                if (matcher.Invoke(tokens, isLast ? required : false, ref index, out parseStatus, out expr))
                    return true;
                else if (parseStatus.Intercept)
                    return false;
            }

            return false;
        }
    }
}
