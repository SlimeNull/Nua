using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public abstract class ValueAccessTailSyntax : Syntax
{
    public NuaValue? Evaluate(NuaContext context, Expr expr)
    {
        return Evaluate(context, expr.Evaluate(context));
    }
    public CompiledSyntax Compile(Expr valueToAccessExpr)
    {
        return Compile(valueToAccessExpr.Compile());
    }

    public abstract NuaValue? Evaluate(NuaContext context, NuaValue? valueToAccess);
    public abstract CompiledSyntax Compile(CompiledSyntax compiledValueToAccess);

}
