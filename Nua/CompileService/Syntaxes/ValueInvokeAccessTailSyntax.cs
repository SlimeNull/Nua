using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ValueInvokeAccessTailSyntax : ValueAccessTailSyntax
{
    public IReadOnlyList<Expr> ParameterExpressions { get; }
    public IReadOnlyList<KeyValuePair<string, Expr>> NamedParameterExpressions { get; }

    public ValueInvokeAccessTailSyntax(
        IEnumerable<Expr> parameterExpressions,
        IEnumerable<KeyValuePair<string, Expr>> namedParameterExpressions)
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

        NuaFunction invocationFunction;
        if (valueToAccess is NuaFunction function)
            invocationFunction = function;
        else if (valueToAccess is NuaTable table &&
            table.GetInvocationFunction(context) is NuaFunction tableInvocationFunction)
            invocationFunction = tableInvocationFunction;
        else
            throw new NuaEvalException("Unable to invoke a non-invocable value");

        var parameters = ParameterExpressions
            .Select(p => p.Evaluate(context))
            .ToArray();
        var namedParameters = NamedParameterExpressions
            .Select(p => new KeyValuePair<string, NuaValue?>(p.Key, p.Value.Evaluate(context)))
            .ToArray();

        var result = invocationFunction.Invoke(context, parameters, namedParameters);

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

            NuaFunction invocationFunction;
            if (valueToAccess is NuaFunction function)
                invocationFunction = function;
            else if (valueToAccess is NuaTable table &&
                table.GetInvocationFunction(context) is NuaFunction tableInvocationFunction)
                invocationFunction = tableInvocationFunction;
            else
                throw new NuaEvalException("Unable to invoke a non-invocable value");

            var parameters = compiledParameters
                .Select(compiled => compiled.Evaluate(context))
                .ToArray();
            var namedParameters = compiledNamedParameters
                .Select(compiled => new KeyValuePair<string, NuaValue?>(compiled.Key, compiled.Value.Evaluate(context)))
                .ToArray();

            var result = invocationFunction.Invoke(context, parameters, namedParameters);

            return result;
        });

        return result;
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;

        foreach (var parameterExpr in ParameterExpressions)
            foreach (var syntax in parameterExpr.TreeEnumerate())
                yield return syntax;
    }
}
