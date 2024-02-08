using System.Collections.Generic;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;


public class ForInExpr : ForExpr
{
    public string ValueName { get; }
    public string? KeyName { get; }
    public Expr IterableExpr { get; }
    public MultiExpr? BodyExpr { get; }

    public ForInExpr(string valueName, string? keyName, Expr iterableExpr, MultiExpr? bodyExpr)
    {
        ValueName = valueName;
        KeyName = keyName;
        IterableExpr = iterableExpr;
        BodyExpr = bodyExpr;
    }

    public override NuaValue? Evaluate(NuaContext context, out EvalState state)
    {
        state = EvalState.None;
        var iterableValue = IterableExpr.Evaluate(context);
        NuaValue? result = null;

        if (iterableValue is NuaTable table)
        {
            foreach (var kv in table)
            {
                context.Set(ValueName, kv.Value);
                if (KeyName != null)
                    context.Set(KeyName, kv.Key);

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
        }
        else if (iterableValue is NuaList list)
        {
            for (int i = 0; i < list.Storage.Count; i++)
            {
                NuaValue? value = list.Storage[i];

                context.Set(ValueName, value);
                if (KeyName != null)
                    context.Set(KeyName, new NuaNumber(i));

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
        }
        else if (iterableValue is NuaString str)
        {
            for (int i = 0; i < str.Value.Length; i++)
            {
                char value = str.Value[i];

                context.Set(ValueName, new NuaString(value.ToString()));
                if (KeyName != null)
                    context.Set(KeyName, new NuaNumber(i));

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
        }
        else
        {
            throw new NuaEvalException("Unable to iterate on a non-iterable value");
        }

        return result;
    }

    public override CompiledProcessSyntax Compile()
    {
        CompiledSyntax compiledIterable = IterableExpr.Compile();
        CompiledProcessSyntax? compiledBody = BodyExpr?.Compile();

        return CompiledProcessSyntax.CreateFromDelegate(
            delegate (NuaContext context, out EvalState state)
            {
                state = EvalState.None;
                var iterableValue = compiledIterable.Evaluate(context);
                NuaValue? result = null;

                if (iterableValue is NuaTable table)
                {
                    foreach (var kv in table)
                    {
                        context.Set(ValueName, kv.Value);
                        if (KeyName != null)
                            context.Set(KeyName, kv.Key);

                        EvalState bodyState = EvalState.None;
                        result = compiledBody?.Evaluate(context, out bodyState);

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
                }
                else if (iterableValue is NuaList list)
                {
                    for (int i = 0; i < list.Storage.Count; i++)
                    {
                        NuaValue? value = list.Storage[i];

                        context.Set(ValueName, value);
                        if (KeyName != null)
                            context.Set(KeyName, new NuaNumber(i));

                        EvalState bodyState = EvalState.None;
                        result = compiledBody?.Evaluate(context, out bodyState);

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
                }
                else if (iterableValue is NuaString str)
                {
                    for (int i = 0; i < str.Value.Length; i++)
                    {
                        char value = str.Value[i];

                        context.Set(ValueName, new NuaString(value.ToString()));
                        if (KeyName != null)
                            context.Set(KeyName, new NuaNumber(i));

                        EvalState bodyState = EvalState.None;
                        result = compiledBody?.Evaluate(context, out bodyState);

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
                }
                else
                {
                    throw new NuaEvalException("Unable to iterate on a non-iterable value");
                }

                return result;
            });
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in IterableExpr.TreeEnumerate())
            yield return syntax;

        if (BodyExpr is not null)
            foreach (var syntax in BodyExpr.TreeEnumerate())
                yield return syntax;
    }
}
