using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;


public class AddExpr : Expr
{
    public Expr Value { get; }
    public IEnumerable<KeyValuePair<AddOperation, Expr>> Operations { get; }

    public AddExpr(Expr left, IEnumerable<KeyValuePair<AddOperation, Expr>> operations)
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
                AddOperation.Min => EvalUtilities.EvalMinus(result, operation.Value.Evaluate(context)),
                AddOperation.Add or _ => EvalUtilities.EvalPlus(result, operation.Value.Evaluate(context)),
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
                AddOperation.Min => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalMinus(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
                AddOperation.Add or _ => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalPlus(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
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
