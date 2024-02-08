using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;


public class OrTailExpr : Expr
{
    public Expr RightExpr { get; }
    public OrTailExpr? NextTailExpr { get; }

    public OrTailExpr(Expr rightExpr, OrTailExpr? nextTailExpr)
    {
        RightExpr = rightExpr;
        NextTailExpr = nextTailExpr;
    }

    public override NuaValue? Evaluate(NuaContext context)
    {
        var rightValue = RightExpr.Evaluate(context);

        if (rightValue == null)
        {
            if (NextTailExpr == null)
                return new NuaBoolean(false);
            else
                return NextTailExpr.Evaluate(context);
        }

        if (rightValue is not NuaBoolean rightBoolean)
            return rightValue;

        if (!rightBoolean.Value)
        {
            if (NextTailExpr == null)
                return new NuaBoolean(false);
            else
                return NextTailExpr.Evaluate(context);
        }

        return new NuaBoolean(true);
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
                return right;
            else
                return compiledNextTail.Evaluate(context);
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
