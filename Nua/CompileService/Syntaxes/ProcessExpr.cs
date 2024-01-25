using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public abstract class ProcessExpr : Expr
    {
        public abstract NuaValue? Eval(NuaContext context, out EvalState state);

        public override NuaValue? Eval(NuaContext context) => Eval(context, out _);

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ProcessExpr? expr)
        {
            expr = null;

            if (BreakExpr.Match(tokens, ref index, out var expr7))
                expr = expr7;
            else if (ContinueExpr.Match(tokens, ref index, out var expr6))
                expr = expr6;
            else if (ForExpr.Match(tokens, ref index, out var expr2))
                expr = expr2;
            else if (IfExpr.Match(tokens, ref index, out var expr1))
                expr = expr1;
            else
                return false;

            return true;
        }
    }
}
