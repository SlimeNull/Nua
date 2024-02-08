using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ValueInvokeAccessTailExpr : ValueAccessTailExpr
{
    public IReadOnlyList<Expr> ParameterExpressions { get; }
    public IReadOnlyList<KeyValuePair<string, Expr>> NamedParameterExpressions { get; }

    public ValueInvokeAccessTailExpr(
        IEnumerable<Expr> parameterExpressions,
        IEnumerable<KeyValuePair<string, Expr>> namedParameterExpressions,
        ValueAccessTailExpr? nextTailExpr) : base(nextTailExpr)
    {
        ParameterExpressions = parameterExpressions
            .ToList()
            .AsReadOnly();
        NamedParameterExpressions = namedParameterExpressions
            .ToList()
            .AsReadOnly();
    }

    public override NuaValue? Evaluate(NuaContext context, NuaValue? valueToAccess)
    {
        if (valueToAccess == null)
            throw new NuaEvalException("Unable to invoke a null value");
        if (valueToAccess is not NuaFunction function)
            throw new NuaEvalException("Unable to invoke a non-function value");

        var parameters = ParameterExpressions
            .Select(p => p.Evaluate(context))
            .ToArray();
        var namedParameters = NamedParameterExpressions
            .Select(p => new KeyValuePair<string, NuaValue?>(p.Key, p.Value.Evaluate(context)))
            .ToArray();

        var result = function.Invoke(context, parameters, namedParameters);

        if (NextTailExpr != null)
            result = NextTailExpr.Evaluate(context, result);

        return result;
    }

    public override CompiledSyntax Compile(CompiledSyntax compiledValueToAccess)
    {
        var compiledParameters = ParameterExpressions
            .Select(p => p.Compile())
            .ToArray();
        var compiledNamedParameters = NamedParameterExpressions
            .Select(p => new KeyValuePair<string, CompiledSyntax>(p.Key, p.Value.Compile()))
            .ToArray();

        CompiledSyntax result = CompiledSyntax.CreateFromDelegate(context =>
        {
            var valueToAccess = compiledValueToAccess.Evaluate(context);

            if (valueToAccess == null)
                throw new NuaEvalException("Unable to invoke a null value");
            if (valueToAccess is not NuaFunction function)
                throw new NuaEvalException("Unable to invoke a non-function value");

            var parameters = compiledParameters
                .Select(compiled => compiled.Evaluate(context))
                .ToArray();
            var namedParameters = compiledNamedParameters
                .Select(compiled => new KeyValuePair<string, NuaValue?>(compiled.Key, compiled.Value.Evaluate(context)))
                .ToArray();

            var result = function.Invoke(context, parameters, namedParameters);

            return result;
        });

        if (NextTailExpr is not null)
            result = NextTailExpr.Compile(result);

        return result;
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;

        foreach (var parameterExpr in ParameterExpressions)
            foreach (var syntax in parameterExpr.TreeEnumerate())
                yield return syntax;

        if (NextTailExpr is not null)
            foreach (var syntax in NextTailExpr.TreeEnumerate())
                yield return syntax;
    }
}
