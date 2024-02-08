using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class GlobalExpr : ProcessExpr
{
    public GlobalExpr(string variableName)
    {
        VariableName = variableName;
    }

    public string VariableName { get; }

    public override NuaValue? Evaluate(NuaContext context, out EvalState state)
    {
        context.TagGlobal(VariableName);
        state = EvalState.None;
        return context.Get(VariableName);
    }

    public override CompiledProcessSyntax Compile()
    {
        return CompiledProcessSyntax.CreateFromDelegate(
            delegate (NuaContext context, out EvalState state)
            {
                context.TagGlobal(VariableName);
                state = EvalState.None;
                return context.Get(VariableName);
            });
    }

}
