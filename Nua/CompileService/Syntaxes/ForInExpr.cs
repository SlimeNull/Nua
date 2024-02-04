using System.Collections.Generic;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class ForInExpr : ForExpr
    {
        public ForInExpr(string valueName, string? keyName, Expr iterable, MultiExpr? body)
        {
            ValueName = valueName;
            KeyName = keyName;
            IterableExpr = iterable;
            BodyExpr = body;
        }

        public string ValueName { get; }
        public string? KeyName { get; }
        public Expr IterableExpr { get; }
        public MultiExpr? BodyExpr { get; }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
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
                        continue;
                    else if (bodyState == EvalState.Break)
                        break;
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
                        continue;
                    else if (bodyState == EvalState.Break)
                        break;
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
                        continue;
                    else if (bodyState == EvalState.Break)
                        break;
                }
            }
            else
            {
                throw new NuaEvalException("Unable to iterate on a non-iterable value");
            }

            state = EvalState.None;
            return result;
        }

    }
}
