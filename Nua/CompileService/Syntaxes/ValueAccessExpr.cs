using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ValueAccessExpr : PrimaryExpr, IAssignableSyntax
{
    public ValueExpr ValueExpr { get; }
    public ValueAccessTailSyntax TailExpr { get; }

    public ValueAccessExpr(ValueExpr valueExpr, ValueAccessTailSyntax tailExpr)
    {
        ValueExpr = valueExpr ?? throw new ArgumentNullException(nameof(valueExpr));
        TailExpr = tailExpr;
    }

    public override NuaValue? Evaluate(NuaContext context)
    {
        return TailExpr.Evaluate(context, ValueExpr);
    }
    public override CompiledSyntax Compile()
    {
        return TailExpr.Compile(ValueExpr);
    }

    public void Assign(NuaContext context, NuaValue? value)
    {
        TailExpr.SetMemberValue(context, ValueExpr, value);
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in ValueExpr.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in TailExpr.TreeEnumerate())
            yield return syntax;
    }
}
