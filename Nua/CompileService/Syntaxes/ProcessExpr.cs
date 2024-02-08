using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public abstract class ProcessExpr : Expr
{
    public abstract NuaValue? Evaluate(NuaContext context, out EvalState state);
    public override NuaValue? Evaluate(NuaContext context) => Evaluate(context, out _);
    public abstract override CompiledProcessSyntax Compile();
}
