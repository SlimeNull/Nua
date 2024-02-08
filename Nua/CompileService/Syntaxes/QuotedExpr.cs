using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class QuotedExpr : ValueExpr
{
    public QuotedExpr(Expr valueExpr)
    {
        ValueExpr = valueExpr;
    }

    public Expr ValueExpr { get; }

    public override NuaValue? Evaluate(NuaContext context) => ValueExpr.Evaluate(context);

    public override CompiledSyntax Compile() => ValueExpr.Compile();

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in ValueExpr.TreeEnumerate())
            yield return syntax;
    }
}
