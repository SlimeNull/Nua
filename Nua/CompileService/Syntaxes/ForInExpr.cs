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

        public override NuaValue? Eval(NuaContext context, out EvalState state)
        {
            NuaContext forContext = new(context);

            var iterableValue = Iterable.Eval(context);
            NuaValue? result = null;

            if (iterableValue is NuaTable table)
            {
                foreach (var kv in table.Storage)
                {
                    forContext.Values.Clear();
                    forContext.Values[ValueName] = kv.Value;
                    if (KeyName != null)
                        forContext.Values[KeyName] = kv.Key;

                    result = Body.Eval(forContext, out var bodyState);

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
                    forContext.Values.Clear();

                    if (value != null)
                        forContext.Values[ValueName] = value;
                    if (KeyName != null)
                        forContext.Values[KeyName] = new NuaNumber(i);

                    result = Body.Eval(forContext, out var bodyState);

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
                    forContext.Values.Clear();

                    forContext.Values[ValueName] = new NuaString(value.ToString());
                    if (KeyName != null)
                        forContext.Values[KeyName] = new NuaNumber(i);

                    result = Body.Eval(forContext, out var bodyState);

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
