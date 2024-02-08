using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ChainExpr : EvaluableSyntax
{
    public ChainExpr(IEnumerable<Expr> expressions)
    {
        Expressions = expressions
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<Expr> Expressions { get; }

    public override NuaValue? Evaluate(NuaContext context)
    {
        NuaValue? value = null;
        foreach (var expr in Expressions)
            value = expr?.Evaluate(context);

        return value;
    }

    public override CompiledSyntax Compile()
    {
        List<CompiledSyntax> compiledSyntaxes = new(Expressions.Count);
        foreach (var expr in Expressions)
            compiledSyntaxes.Add(expr.Compile());

        return CompiledSyntax.CreateFromDelegate((context) =>
        {
            NuaValue? result = null;
            foreach (var compiledSyntax in compiledSyntaxes)
                result = compiledSyntax.Evaluate(context);

            return result;
        });
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;

        foreach (var expr in Expressions)
            foreach (var syntax in expr.TreeEnumerate())
                yield return syntax;
    }
}
