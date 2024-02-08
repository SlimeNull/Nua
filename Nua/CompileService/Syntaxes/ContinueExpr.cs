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

    }
}
