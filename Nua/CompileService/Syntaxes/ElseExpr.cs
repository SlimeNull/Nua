using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ElseExpr : ProcessExpr
{
    public MultiExpr? BodyExpr { get; }

    public ElseExpr(MultiExpr? bodyExpr)
    {
        BodyExpr = bodyExpr;
    }

    public override NuaValue? Evaluate(NuaContext context, out EvalState state) => throw new InvalidOperationException();
    public override CompiledProcessSyntax Compile() => throw new InvalidOperationException();

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;

        if (BodyExpr is not null)
            foreach (var syntax in BodyExpr.TreeEnumerate())
                yield return syntax;
    }
}
