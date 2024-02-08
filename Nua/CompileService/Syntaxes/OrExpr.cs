using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class OrExpr : Expr
{
    public OrExpr(Expr leftExpr, OrTailSyntax tailExpr)
    {
        LeftExpr = leftExpr;
        TailExpr = tailExpr;
    }

    public Expr LeftExpr { get; }
    public OrTailSyntax TailExpr { get; }

    public override NuaValue? Evaluate(NuaContext context)
    {
        var leftValue = LeftExpr.Evaluate(context);

        if (leftValue == null)
            return TailExpr.Evaluate(context);

        if (leftValue is not NuaBoolean leftBoolean)
            return leftValue;

        if (!leftBoolean.Value)
            return TailExpr.Evaluate(context);

        return new NuaBoolean(true);
    }

    public override CompiledSyntax Compile()
    {
        var compiledLeft = LeftExpr.Compile();
        var compiledTail = TailExpr.Compile();

        return CompiledSyntax.CreateFromDelegate((context) =>
        {
            var right = compiledLeft.Evaluate(context);
            if (EvalUtilities.ConditionTest(right))
                return right;
            else
                return compiledTail.Evaluate(context);
        });
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in LeftExpr.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in TailExpr.TreeEnumerate())
            yield return syntax;
    }
}
