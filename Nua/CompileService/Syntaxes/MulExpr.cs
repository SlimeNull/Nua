using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class MulExpr : Expr
{
    public Expr Value { get; }
    public IEnumerable<KeyValuePair<MulOperation, Expr>> Operations { get; }

    public MulExpr(Expr left, IEnumerable<KeyValuePair<MulOperation, Expr>> operations)
    {
        Value = left;
        Operations = operations;
    }

    public override NuaValue? Evaluate(NuaContext context)
    {
        NuaValue? result = Value.Evaluate(context);

        foreach (var operation in Operations)
        {
            result = operation.Key switch
            {
                MulOperation.Div => EvalUtilities.EvalDivide(result, operation.Value.Evaluate(context)),
                MulOperation.Pow => EvalUtilities.EvalPower(result, operation.Value.Evaluate(context)),
                MulOperation.Mod => EvalUtilities.EvalMod(result, operation.Value.Evaluate(context)),
                MulOperation.DivInt => EvalUtilities.EvalDivideInt(result, operation.Value.Evaluate(context)),
                MulOperation.Mul or _ => EvalUtilities.EvalMultiply(result, operation.Value.Evaluate(context)),
            };
        }

        return result;
    }

    public override CompiledSyntax Compile()
    {
        var result = Value.Compile();

        foreach (var operation in Operations)
        {
            var compiledLeft = result;
            var compiledRight = operation.Value.Compile();
            result = operation.Key switch
            {
                MulOperation.Div => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalDivide(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
                MulOperation.Pow => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalPower(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
                MulOperation.Mod => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalMod(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
                MulOperation.DivInt => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalDivideInt(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
                MulOperation.Mul or _ => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalMultiply(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
            };
        }

        return result;
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in Value.TreeEnumerate())
            yield return syntax;
        foreach (var tail in Operations)
            foreach (var syntax in tail.Value.TreeEnumerate())
                yield return syntax;
    }
}
