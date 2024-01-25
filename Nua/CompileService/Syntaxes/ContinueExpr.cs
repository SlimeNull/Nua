using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ContinueExpr : ProcessExpr
    {
        public override NuaValue? Eval(NuaContext context, out EvalState state)
        {
            state = EvalState.Continue;
            return null;
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ContinueExpr? expr)
        {
            expr = null;

            if (index < 0 || index >= tokens.Count)
                return false;
            if (tokens[index].Kind != TokenKind.KwdContinue)
                return false;

            index++;
            expr = new();
            return true;
        }
    }
}
