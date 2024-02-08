using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class FuncExpr : ValueExpr
{
    public IReadOnlyList<string> ParameterNames { get; }
    public MultiExpr? BodyExpr { get; }

    public IReadOnlyList<Token>? ParameterNameTokens { get; }

    public FuncExpr(IEnumerable<string> parameterNames, MultiExpr? bodyExpr)
    {
        ParameterNames = parameterNames.ToList().AsReadOnly();
        BodyExpr = bodyExpr;
    }

    public FuncExpr(IEnumerable<Token> parameterNameTokens, MultiExpr? bodyExpr)
    {
        foreach (var token in parameterNameTokens)
            if (token.Value is null)
                throw new ArgumentException("Value of name token is null", nameof(parameterNameTokens));

        ParameterNames = parameterNameTokens.Select(t => t.Value!).ToList().AsReadOnly();
        BodyExpr = bodyExpr;
        ParameterNameTokens = parameterNameTokens.ToList().AsReadOnly();
    }

    public override NuaValue? Evaluate(NuaContext context)
        => new NuaNativeFunction(BodyExpr, ParameterNames.ToArray());

    public override CompiledSyntax Compile()
    {
        CompiledProcessSyntax? compiledBody = BodyExpr?.Compile();
        NuaCompiledNativeFunction? bufferedValue = null;

        return CompiledSyntax.CreateFromDelegate(context =>
        {
            return bufferedValue ??= new NuaCompiledNativeFunction(compiledBody, ParameterNames.ToArray());
        });
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;

        if (BodyExpr is not null)
            foreach (var syntax in BodyExpr.TreeEnumerate())
                yield return syntax;
    }
}
