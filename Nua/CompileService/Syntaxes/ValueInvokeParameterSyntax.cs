using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes;

public class ValueInvokeParameterSyntax : Syntax
{
    public ValueInvokeParameterSyntax(Expr valueExpr, string? name)
    {
        ValueExpr = valueExpr;
        Name = name;
    }

    public Expr ValueExpr { get; }
    public string? Name { get; }
}
