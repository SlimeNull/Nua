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

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
expr = null;
            parseStatus.Message = null;

            if (!TokenMatch(tokens, required, TokenKind.KwdBreak, ref index, out parseStatus.RequireMoreTokens, out _))
                return false;

            index++;
            expr = new BreakExpr();
            return true;
        }
    }
}
