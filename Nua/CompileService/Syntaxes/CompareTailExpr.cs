using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class CompareTailExpr : Expr
{
    public Expr RightExpr { get; }
    public CompareOperation Operation { get; }
    public CompareTailExpr? NextTailExpr { get; }

    public CompareTailExpr(Expr rightExpr, CompareOperation operation, CompareTailExpr? nextTailExpr)
    {
        RightExpr = rightExpr;
        Operation = operation;
        NextTailExpr = nextTailExpr;
    }

    public NuaValue? Evaluate(NuaContext context, NuaValue? leftValue)
    {
        NuaValue? rightValue = RightExpr.Evaluate(context);

        NuaValue? result = Operation switch
        {
            CompareOperation.LessThan => EvalUtilities.EvalLessThan(leftValue, rightValue),
            CompareOperation.GreaterThan => EvalUtilities.EvalGreaterThan(leftValue, rightValue),
            CompareOperation.LessEqual => EvalUtilities.EvalLessEqual(leftValue, rightValue),
            CompareOperation.GreaterEqual => EvalUtilities.EvalGreaterEqual(leftValue, rightValue),
            _ => null,
        };

        if (NextTailExpr is not null)
            result = NextTailExpr.Evaluate(context, result);

        return result;
    }
    public NuaValue? Evaluate(NuaContext context, Expr left)
    {
        return Evaluate(context, left.Evaluate(context));
    }

    public CompiledSyntax Compile(CompiledSyntax compiledLeft)
    {
        var compiledRight = RightExpr.Compile();

        var result = Operation switch
        {
            CompareOperation.LessThan => CompiledSyntax.CreateFromDelegate((context) => EvalUtilities.EvalLessThan(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
            CompareOperation.GreaterThan => CompiledSyntax.CreateFromDelegate((context) => EvalUtilities.EvalGreaterThan(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
            CompareOperation.LessEqual => CompiledSyntax.CreateFromDelegate((context) => EvalUtilities.EvalLessEqual(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
            CompareOperation.GreaterEqual => CompiledSyntax.CreateFromDelegate((context) => EvalUtilities.EvalGreaterEqual(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
            _ => throw new Exception("Invalid compare operation")
        };

        if (NextTailExpr is not null)
            result = NextTailExpr.Compile(result);

        return result;
    }
    public CompiledSyntax Compile(Expr leftExpr)
        => Compile(leftExpr.Compile());

    public override NuaValue? Evaluate(NuaContext context) => throw new InvalidOperationException();
    public override CompiledSyntax Compile() => throw new InvalidOperationException();

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
