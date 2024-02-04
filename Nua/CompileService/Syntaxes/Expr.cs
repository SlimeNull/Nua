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

        public static bool MatchAny(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
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

        public delegate bool Matcher(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr);
    }
}
