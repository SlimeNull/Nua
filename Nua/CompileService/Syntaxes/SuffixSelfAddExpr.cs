using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class SuffixSelfAddExpr : PrimaryExpr
{
    public Expr SelfExpr { get; }
    public bool Negative { get; }

    public SuffixSelfAddExpr(Expr selfExpr, bool negative)
    {
        SelfExpr = selfExpr;
        Negative = negative;
    }

    public override NuaValue? Evaluate(NuaContext context)
    {
        var addValue = !Negative ? 1 : 0;

        if (SelfExpr is ValueAccessExpr memberAccessExpr)
        {
            var self = memberAccessExpr.Evaluate(context);

            if (self == null)
                throw new NuaEvalException("Unable to apply self-increment on a null value");
            if (self is not NuaNumber number)
                throw new NuaEvalException("Unable to apply self-increment on a non-number value");

            number.Value += addValue;
            return self;
        }
        else if (SelfExpr is VariableExpr variableExpr)
        {
            var self = variableExpr.Evaluate(context);

            if (self == null)
                throw new NuaEvalException("Unable to apply self-increment on a null value");
            if (self is not NuaNumber number)
                throw new NuaEvalException("Unable to apply self-increment on a non-number value");

            number.Value += addValue;
            return self;
        }
        else
        {
            throw new NuaEvalException("Only Table member, List element and Variable can be increased");
        }
    }

    public override CompiledSyntax Compile()
    {
        var compiledSelf = SelfExpr.Compile();
        var addValue = !Negative ? 1 : 0;

        if (SelfExpr is ValueAccessExpr memberAccessExpr)
        {
            return CompiledSyntax.CreateFromDelegate(context =>
            {
                var self = compiledSelf.Evaluate(context);

                if (self == null)
                    throw new NuaEvalException("Unable to apply self-increment on a null value");
                if (self is not NuaNumber number)
                    throw new NuaEvalException("Unable to apply self-increment on a non-number value");

                number.Value += addValue;
                return self;
            });
        }
        else if (SelfExpr is VariableExpr variableExpr)
        {
            return CompiledSyntax.CreateFromDelegate(context =>
            {
                var self = compiledSelf.Evaluate(context);

                if (self == null)
                    throw new NuaEvalException("Unable to apply self-increment on a null value");
                if (self is not NuaNumber number)
                    throw new NuaEvalException("Unable to apply self-increment on a non-number value");

                number.Value += addValue;
                return self;
            });
        }
        else
        {
            throw new NuaCompileException("Only Table member, List element and Variable can be increased");
        }
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in SelfExpr.TreeEnumerate())
            yield return syntax;
    }
}
