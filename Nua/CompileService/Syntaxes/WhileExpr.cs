using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class WhileExpr : ProcessExpr
{
    public Expr ConditionExpr { get; }
    public MultiExpr? BodyExpr { get; }

    public WhileExpr(Expr conditionExpr, MultiExpr? bodyExpr)
    {
        ConditionExpr = conditionExpr;
        BodyExpr = bodyExpr;
    }

    public override NuaValue? Evaluate(NuaContext context, out EvalState state)
    {
        state = EvalState.None;
        NuaValue? result = null;
        while (NuaUtilities.ConditionTest(ConditionExpr.Evaluate(context)))
        {
            EvalState bodyState = EvalState.None;
            result = BodyExpr?.Evaluate(context, out bodyState);

            if (bodyState == EvalState.Continue)
            {
                continue;
            }
            else if (bodyState == EvalState.Break)
            {
                break;
            }
            else if (bodyState == EvalState.Return)
            {
                state = EvalState.Return;
                break;
            }
        }

        return result;
    }

    public override CompiledProcessSyntax Compile()
    {
        var compiledCondition = ConditionExpr.Compile();
        var compiledBody = BodyExpr?.Compile();

        return CompiledProcessSyntax.CreateFromDelegate(
            delegate (NuaContext context, out EvalState state)
            {
                NuaValue? result = null;
                state = EvalState.None;
                while (EvalUtilities.ConditionTest(compiledCondition.Evaluate(context)))
                {
                    EvalState bodyState = EvalState.None;
                    result = BodyExpr?.Evaluate(context, out bodyState);

                    if (bodyState == EvalState.Continue)
                    {
                        continue;
                    }
                    else if (bodyState == EvalState.Break)
                    {
                        break;
                    }
                    else if (bodyState == EvalState.Return)
                    {
                        state = EvalState.Return;
                        break;
                    }
                }

                return result;
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
    }
}
