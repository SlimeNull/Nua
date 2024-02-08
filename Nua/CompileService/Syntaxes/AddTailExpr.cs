using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class AddTailExpr : Expr
{
    public Expr RightExpr { get; }
    public AddOperation Operation { get; }
    public AddTailExpr? NextTailExpr { get; }

    public AddTailExpr(Expr rightExpr, AddOperation operation, AddTailExpr? nextTailExpr)
    {
        RightExpr = rightExpr;
        Operation = operation;
        NextTailExpr = nextTailExpr;
    }

    public NuaValue? Evaluate(NuaContext context, NuaValue? left)
    {
        var rightValue =  RightExpr.Evaluate(context);

        NuaValue? result = Operation switch
        {
            AddOperation.Min => EvalUtilities.EvalMinus(left, rightValue),
            AddOperation.Add or _ => EvalUtilities.EvalPlus(left, rightValue),
        };

        if (NextTailExpr is not null)
            result = NextTailExpr.Evaluate(context, result);

        return result;
    }
    public NuaValue? Evaluate(NuaContext context, Expr leftExpr)
    {
        return Evaluate(context, leftExpr.Evaluate(context));
    }

    public CompiledSyntax Compile(CompiledSyntax compiledLeft)
    {
        var compiledRight = RightExpr.Compile();

        CompiledSyntax result = Operation switch
        {
            AddOperation.Min => CompiledSyntax.CreateFromDelegate((context) => EvalUtilities.EvalMinus(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
            AddOperation.Add or _ => CompiledSyntax.CreateFromDelegate((context) => EvalUtilities.EvalPlus(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
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
