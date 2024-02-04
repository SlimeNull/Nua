using System.Collections.Generic;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class ForInExpr : ForExpr
    {
        public ForInExpr(string valueName, string? keyName, Expr iterable, MultiExpr body)
        {
            ValueName = valueName;
            KeyName = keyName;
            Iterable = iterable;
            Body = body;
        }

        public string ValueName { get; }
        public string? KeyName { get; }
        public Expr Iterable { get; }
        public MultiExpr Body { get; }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            var iterableValue = Iterable.Evaluate(context);
            NuaValue? result = null;

            if (iterableValue is NuaTable table)
            {
                foreach (var kv in table)
                {
                    context.Set(ValueName, kv.Value);
                    if (KeyName != null)
                        context.Set(KeyName, kv.Key);

                    result = Body.Evaluate(context, out var bodyState);

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

                    result = Body.Evaluate(context, out var bodyState);

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

                    result = Body.Evaluate(context, out var bodyState);

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
