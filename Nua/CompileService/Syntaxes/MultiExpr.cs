using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class MultiExpr : EvaluableSyntax
{
    public MultiExpr(IEnumerable<Expr> expressions)
    {
        Expressions = expressions
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<Expr> Expressions { get; }

    public NuaValue? Evaluate(NuaContext context, out EvalState state)
    {
        NuaValue? value = null;
        foreach (var expr in Expressions)
        {
            if (expr is ProcessExpr processExpr)
            {
                value = processExpr.Evaluate(context, out var subState);

                if (subState != EvalState.None)
                {
                    state = subState;
                    return value;
                }
            }
            else
            {
                value = expr?.Evaluate(context);
            }
        }

        state = EvalState.None;
        return value;
    }

    public override CompiledProcessSyntax Compile()
    {
        List<CompiledSyntax> compiledSyntaxes = new(Expressions.Count);
        foreach (var expr in Expressions)
            compiledSyntaxes.Add(expr.Compile());

        return CompiledProcessSyntax.CreateFromDelegate(
            delegate (NuaContext context, out EvalState state)
            {
                NuaValue? value = null;
                state = EvalState.None;
                foreach (var compiledSyntax in compiledSyntaxes)
                {
                    if (compiledSyntax is CompiledProcessSyntax compiledProcessSyntax)
                    {
                        value = compiledProcessSyntax.Evaluate(context, out state);

                        if (state != EvalState.None)
                            return value;
                    }
                    else
                    {
                        value = compiledSyntax.Evaluate(context);
                    }
                }

                return value;
            });
    }

    public override NuaValue? Evaluate(NuaContext context)
    {
        return Evaluate(context, out _);
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
