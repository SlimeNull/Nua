using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public abstract class ProcessExpr : Expr
    {
        public abstract NuaValue? Eval(NuaContext context, out EvalState state);

        public override NuaValue? Eval(NuaContext context) => Eval(context, out _);

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            return
                BreakExpr.Match(tokens, ref index, out expr) ||
                ContinueExpr.Match(tokens, ref index, out expr) ||
                ForExpr.Match(tokens, ref index, out expr) ||
                IfExpr.Match(tokens, ref index, out expr) ||
                UnaryExpr.Match(tokens, ref index, out expr);
        }
    }
}
