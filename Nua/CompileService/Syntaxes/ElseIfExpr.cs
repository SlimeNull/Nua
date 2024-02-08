using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ElseIfExpr : ProcessExpr
{
    public Expr ConditionExpr { get; }
    public MultiExpr? BodyExpr { get; }

    public ElseIfExpr(Expr conditionExpr, MultiExpr? bodyExpr)
    {
        ConditionExpr = conditionExpr;
        BodyExpr = bodyExpr;
    }

    public override NuaValue? Evaluate(NuaContext context, out EvalState state) => throw new InvalidOperationException();
    public override CompiledProcessSyntax Compile() => throw new InvalidOperationException();

    public override IEnumerable<Syntax> TreeEnumerate()
    {

        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in ConditionExpr.TreeEnumerate())
            yield return syntax;

        if (BodyExpr is not null)
            foreach (var syntax in BodyExpr.TreeEnumerate())
                yield return syntax;
    }
}
