using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public abstract class ProcessExpr : Expr
    {
        static readonly Matcher[] matchers = new Matcher[]
        {
            ReturnExpr.Match,
            BreakExpr.Match,
            ContinueExpr.Match,
            ForExpr.Match,
            IfExpr.Match,
            UnaryExpr.Match,
        };

        public abstract NuaValue? Evaluate(NuaContext context, out EvalState state);
        public override NuaValue? Evaluate(NuaContext context) => Evaluate(context, out _);

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
