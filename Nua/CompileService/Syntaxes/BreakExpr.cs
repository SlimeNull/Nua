using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class BreakExpr : ProcessExpr
    {
        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            state = EvalState.Break;
            return null;
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            message = null;

            if (!TokenMatch(tokens, required, TokenKind.KwdBreak, ref index, out requireMoreTokens, out _))
                return false;

            index++;
            expr = new BreakExpr();
            return true;
        }
    }
}
