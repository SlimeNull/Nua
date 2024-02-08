using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class AndTailExpr : Expr
{
    public Expr RightExpr { get; }
    public AndTailExpr? NextTailExpr { get; }

    public AndTailExpr(Expr rightExpr, AndTailExpr? nextTailExpr)
    {
        RightExpr = rightExpr;
        NextTailExpr = nextTailExpr;
    }

    public override NuaValue? Evaluate(NuaContext context)
    {
        var rightValue = RightExpr.Evaluate(context);

        if (NextTailExpr == null)
            return rightValue;

        if (EvalUtilities.ConditionTest(rightValue))
        {
            return NextTailExpr.Evaluate(context);
        }

        return rightValue;
    }

    public override CompiledSyntax Compile()
    {
        var compiledRight = RightExpr.Compile();

        if (NextTailExpr == null)
            return compiledRight;

        var compiledNextTail = NextTailExpr.Compile();

        return CompiledSyntax.CreateFromDelegate((context) =>
        {
            var right = compiledRight.Evaluate(context);
            if (EvalUtilities.ConditionTest(right))
                return compiledNextTail.Evaluate(context);
            else
                return right;
        });
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in RightExpr.TreeEnumerate())
            yield return syntax;

        if (NextTailExpr is not null)
            foreach (var syntax in NextTailExpr.TreeEnumerate())
                yield return syntax;
    }
}
