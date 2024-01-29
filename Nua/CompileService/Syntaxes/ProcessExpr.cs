using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public abstract class ProcessExpr : Expr
    {
        public abstract NuaValue? Evaluate(NuaContext context, out EvalState state);

        public override NuaValue? Evaluate(NuaContext context) => Evaluate(context, out _);

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            return
                ReturnExpr.Match(tokens, ref index, out expr) ||
                BreakExpr.Match(tokens, ref index, out expr) ||
                ContinueExpr.Match(tokens, ref index, out expr) ||
                ForExpr.Match(tokens, ref index, out expr) ||
                IfExpr.Match(tokens, ref index, out expr) ||
                UnaryExpr.Match(tokens, ref index, out expr);
        }
    }
}
