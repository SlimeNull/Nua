using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ConstExpr : ValueExpr
{
    public NuaValue? Value { get; }

    public ConstExpr(NuaValue? value)
    {
        Value = value;
    }

    public override NuaValue? Evaluate(NuaContext context) => Value;

    public override CompiledSyntax Compile() => CompiledSyntax.Create(Value);

}
