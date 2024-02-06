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
            WhileExpr.Match,
            ForExpr.Match,
            IfExpr.Match,
            GlobalExpr.Match,
            UnaryExpr.Match,
        };

        public abstract NuaValue? Evaluate(NuaContext context, out EvalState state);
        public override NuaValue? Evaluate(NuaContext context) => Evaluate(context, out _);
        public abstract override CompiledProcessSyntax Compile();

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
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
