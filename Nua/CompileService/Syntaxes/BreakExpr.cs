using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class BreakExpr : ProcessExpr
    {
        public override NuaValue? Eval(NuaContext context, out EvalState state)
        {
            state = EvalState.Break;
            return null;
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out BreakExpr? expr)
        {
            expr = null;

            if (index < 0 || index >= tokens.Count)
                return false;
            if (tokens[index].Kind != TokenKind.KwdBreak)
                return false;

            index++;
            expr = new();
            return true;
        }
    }
}
