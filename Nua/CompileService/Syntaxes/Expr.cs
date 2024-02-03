using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Nua.CompileService.Syntaxes
{

    public abstract class Expr : Syntax
    {
        static readonly Matcher[] matchers = new Matcher[]
        {
            AssignExpr.Match,
            OrExpr.Match,
        };

        public static bool MatchAny(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
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

        public delegate bool Matcher(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr);
    }
}
