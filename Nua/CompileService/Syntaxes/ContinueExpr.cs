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

        public override CompiledProcessSyntax Compile()
        {
            return CompiledProcessSyntax.Create(null, EvalState.Continue);
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            parseStatus.Message = null;

            if (!TokenMatch(tokens, required, TokenKind.KwdContinue, ref index, out parseStatus.RequireMoreTokens, out _))
                return false;

            expr = new ContinueExpr();
            return true;
        }
    }
}
