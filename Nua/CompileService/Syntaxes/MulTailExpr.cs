using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class MulTailExpr : Expr
{
    public Expr RightExpr { get; }
    public MulOperation Operation { get; }
    public MulTailExpr? NextTailExpr { get; }

    public MulTailExpr(Expr rightExpr, MulOperation operation, MulTailExpr? nextTailExpr)
    {
        RightExpr = rightExpr;
        Operation = operation;
        NextTailExpr = nextTailExpr;
    }

    public NuaValue? Evaluate(NuaContext context, NuaValue? left)
    {
        var rightValue = RightExpr.Evaluate(context);

        NuaValue? result = Operation switch
        {
            MulOperation.Mul => EvalUtilities.EvalMultiply(left, rightValue),
            MulOperation.Div => EvalUtilities.EvalDivide(left, rightValue),
            MulOperation.Pow => EvalUtilities.EvalPower(left, rightValue),
            MulOperation.Mod => EvalUtilities.EvalMod(left, rightValue),
            MulOperation.DivInt => EvalUtilities.EvalDivideInt(left, rightValue),
            _ => EvalUtilities.EvalMultiply(left, rightValue),
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

        CompiledSyntax result = Operation switch
        {
            MulOperation.Div => CompiledSyntax.CreateFromDelegate((context) =>
            {
                var left = compiledLeft.Evaluate(context);
                var right = compiledRight.Evaluate(context);

                return EvalUtilities.EvalDivide(left, right);
            })
            ,
            MulOperation.Pow => CompiledSyntax.CreateFromDelegate((context) =>
            {
                var left = compiledLeft.Evaluate(context);
                var right = compiledRight.Evaluate(context);

                return EvalUtilities.EvalPower(left, right);
            })
            ,
            MulOperation.Mod => CompiledSyntax.CreateFromDelegate((context) =>
            {
                var left = compiledLeft.Evaluate(context);
                var right = compiledRight.Evaluate(context);

                return EvalUtilities.EvalMod(left, right);
            })
            ,
            MulOperation.DivInt => CompiledSyntax.CreateFromDelegate((context) =>
            {
                var left = compiledLeft.Evaluate(context);
                var right = compiledRight.Evaluate(context);

                return EvalUtilities.EvalDivideInt(left, right);
            })
            ,
            MulOperation.Mul or _ => CompiledSyntax.CreateFromDelegate((context) =>
            {
                var left = compiledLeft.Evaluate(context);
                var right = compiledRight.Evaluate(context);

                return EvalUtilities.EvalMultiply(left, right);
            })
        };

        if (NextTailExpr is not null)
            result = NextTailExpr.Compile(result);

        return result;
    }
    public CompiledSyntax Compile(Expr leftExpr)
    {
        var compiledLeft = leftExpr.Compile();
        var compiledRight = RightExpr.Compile();

        CompiledSyntax result = Operation switch
        {
            MulOperation.Div => CompiledSyntax.CreateFromDelegate((context) =>
            {
                var left = compiledLeft.Evaluate(context);
                var right = compiledRight.Evaluate(context);

                return EvalUtilities.EvalDivide(left, right);
            })
            ,
            MulOperation.Pow => CompiledSyntax.CreateFromDelegate((context) =>
            {
                var left = compiledLeft.Evaluate(context);
                var right = compiledRight.Evaluate(context);

                return EvalUtilities.EvalPower(left, right);
            })
            ,
            MulOperation.Mod => CompiledSyntax.CreateFromDelegate((context) =>
            {
                var left = compiledLeft.Evaluate(context);
                var right = compiledRight.Evaluate(context);

                return EvalUtilities.EvalMod(left, right);
            })
            ,
            MulOperation.DivInt => CompiledSyntax.CreateFromDelegate((context) =>
            {
                var left = compiledLeft.Evaluate(context);
                var right = compiledRight.Evaluate(context);

                return EvalUtilities.EvalDivideInt(left, right);
            })
            ,
            MulOperation.Mul or _ => CompiledSyntax.CreateFromDelegate((context) =>
            {
                var left = compiledLeft.Evaluate(context);
                var right = compiledRight.Evaluate(context);

                return EvalUtilities.EvalMultiply(left, right);
            })
        };

        if (NextTailExpr is not null)
            result = NextTailExpr.Compile(result);

        return result;
    }

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
