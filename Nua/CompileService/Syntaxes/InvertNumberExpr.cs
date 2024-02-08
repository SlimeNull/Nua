using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class InvertNumberExpr : UnaryExpr
{
    public Expr ValueExpr { get; }

    public InvertNumberExpr(Expr valueExpr)
    {
        ValueExpr = valueExpr;
    }

    public override NuaValue? Evaluate(NuaContext context)
    {
        var value = ValueExpr.Evaluate(context);

        if (value == null)
            throw new NuaEvalException("Unable to take negation of null value");
        if (value is not NuaNumber number)
            throw new NuaEvalException("Unable to take negation of non-number value");

        return new NuaNumber(-number.Value);
    }

    public override CompiledSyntax Compile()
    {
        CompiledSyntax compiledValue = ValueExpr.Compile();

        return CompiledSyntax.CreateFromDelegate(context =>
        {
            var value = compiledValue.Evaluate(context);

            if (value == null)
                throw new NuaEvalException("Unable to take negation of null value");
            if (value is not NuaNumber number)
                throw new NuaEvalException("Unable to take negation of non-number value");

            return new NuaNumber(-number.Value);
        });
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        if (ValueExpr is not null)
            foreach (var syntax in ValueExpr.TreeEnumerate())
                yield return syntax;
    }
}
