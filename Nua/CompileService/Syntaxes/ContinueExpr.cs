using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ContinueExpr : ProcessExpr
    {
        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            state = EvalState.Continue;
            return null;
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            message = null;

            if (!TokenMatch(tokens, required, TokenKind.KwdContinue, ref index, out requireMoreTokens, out _))
                return false;

            index++;
            expr = new ContinueExpr();
            return true;
        }
    }
 }
