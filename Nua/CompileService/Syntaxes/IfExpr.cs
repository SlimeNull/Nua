using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class IfExpr : ProcessExpr
{
    public Expr ConditionExpr { get; }
    public MultiExpr? BodyExpr { get; }
    public IReadOnlyList<ElseIfExpr>? ElseIfExpressions { get; }
    public ElseExpr? ElseExpr { get; }

    public IfExpr(Expr conditionExpr, MultiExpr? bodyExpr, IEnumerable<ElseIfExpr>? elseIfExpressions, ElseExpr? elseExpr)
    {
        ConditionExpr = conditionExpr;
        BodyExpr = bodyExpr;
        ElseIfExpressions = elseIfExpressions?.ToList()?.AsReadOnly();
        ElseExpr = elseExpr;
    }

    public override NuaValue? Evaluate(NuaContext context, out EvalState state)
    {
        state = EvalState.None;

        if (NuaUtilities.ConditionTest(ConditionExpr.Evaluate(context)))
        {
            return BodyExpr?.Evaluate(context, out state);
        }
        else
        {
            if (ElseIfExpressions != null)
            {
                foreach (var elseif in ElseIfExpressions)
                {
                    if (EvalUtilities.ConditionTest(elseif.ConditionExpr.Evaluate(context)))
                    {
                        return elseif.BodyExpr?.Evaluate(context, out state);
                    }
                }
            }

            return ElseExpr?.BodyExpr?.Evaluate(context, out state);
        }
    }

    public override CompiledProcessSyntax Compile()
    {
        CompiledSyntax compiledCondition = ConditionExpr.Compile();
        CompiledProcessSyntax? compiledBody = BodyExpr?.Compile();

        List<(CompiledSyntax Condition, CompiledProcessSyntax? Body)> compiledElseIfSyntaxes = new();
        CompiledProcessSyntax? compiledElseBody = ElseExpr?.BodyExpr?.Compile() as CompiledProcessSyntax;

        if (ElseIfExpressions is not null)
            foreach (var elseif in ElseIfExpressions)
                compiledElseIfSyntaxes.Add((elseif.ConditionExpr.Compile(), elseif.BodyExpr?.Compile()));

        return CompiledProcessSyntax.CreateFromDelegate(
            delegate (NuaContext context, out EvalState state)
            {
                state = EvalState.None;
                if (EvalUtilities.ConditionTest(compiledCondition.Evaluate(context)))
                {
                    return compiledBody?.Evaluate(context, out state);
                }
                else
                {
                    foreach (var compiledElseIf in compiledElseIfSyntaxes)
                    {
                        if (EvalUtilities.ConditionTest(compiledElseIf.Condition.Evaluate(context)))
                        {
                            return compiledElseIf.Body?.Evaluate(context, out state);
                        }
                    }

                    return compiledElseBody?.Evaluate(context, out state);
                }
            });
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;

        foreach (var syntax in ConditionExpr.TreeEnumerate())
            yield return syntax;

        if (BodyExpr is not null)
            foreach (var syntax in BodyExpr.TreeEnumerate())
                yield return syntax;

        if (ElseIfExpressions is not null)
            foreach (var expr in ElseIfExpressions)
                foreach (var syntax in expr.TreeEnumerate())
                    yield return syntax;

        if (ElseExpr is not null)
            foreach (var syntax in ElseExpr.TreeEnumerate())
                yield return syntax;
    }
}
