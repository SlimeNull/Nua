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

        public override CompiledProcessSyntax Compile()
        {
            return CompiledProcessSyntax.Create(null, EvalState.Break);
        }

    }
}
