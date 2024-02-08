using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public abstract class EvaluableSyntax : Syntax
{
    public abstract NuaValue? Evaluate(NuaContext context);
    public abstract CompiledSyntax Compile();
}
