using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Nua.Types;

namespace Nua.Stdlib
{
    public class ListOperations : NuaNativeTable
    {
        private ListOperations() { }

        public static ListOperations Create()
        {
            return new ListOperations()
            {
                Storage =
                {
                    [new NuaString("add")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "add";

                            var list = OperationsHelper.TakeListParam(functionName, parameters, 0);
                            OperationsHelper.EnsureParamIndex(functionName, parameters, "any", 1);

                            list.Storage.Add(parameters[1]);
                            return list;
                        }),
                    [new NuaString("add_range")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "add_range";

                            var list = OperationsHelper.TakeListParam(functionName, parameters, 0);
                            var sublist = OperationsHelper.TakeListParam(functionName, parameters, 1);

                            list.Storage.AddRange(sublist.Storage);
                            return list;
                        }),
                    [new NuaString("clear")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "clear";

                            var list = OperationsHelper.TakeListParam(functionName, parameters, 0);

                            list.Storage.Clear();
                            return list;
                        }),
                    [new NuaString("find")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "find";

                            var list = OperationsHelper.TakeListParam(functionName, parameters, 0);
                            OperationsHelper.EnsureParamIndex(functionName, parameters, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", 1);

                            if (parameters[1] is NuaNumber nuaStartIndex)
                            {
                                OperationsHelper.EnsureParamIndex(functionName, parameters, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", 2);
                                if (parameters[2] is NuaNumber nuaCount)
                                {
                                    var nuaMatch = OperationsHelper.TakeFunctionParam(functionName, parameters, 3);

                                    int
                                        startIndex = (int)nuaStartIndex.Value,
                                        count = Math.Min(list.Storage.Count - startIndex, (int)nuaCount.Value);

                                    for (int i = startIndex, j = 0; j < count; i++, j++)
                                        if (NuaUtilities.ConditionTest(nuaMatch.Invoke(context, list.Storage[i])))
                                            return list.Storage[i];

                                    return null;
                                }
                                else if (parameters[2] is NuaFunction nuaMatch)
                                {
                                    int
                                        startIndex = (int)nuaStartIndex.Value;

                                    for (int i = startIndex; i < list.Storage.Count; i++)
                                        if (NuaUtilities.ConditionTest(nuaMatch.Invoke(context, list.Storage[i])))
                                            return list.Storage[i];

                                    return null;
                                }
                                else
                                {
                                    throw OperationsHelper.BuildParamException(functionName, 2, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", parameters[2]?.TypeName ?? "null");
                                }

                            }
                            else if (parameters[1] is NuaFunction nuaMatch)
                            {
                                for (int i = 0; i < list.Storage.Count; i++)
                                    if (NuaUtilities.ConditionTest(nuaMatch.Invoke(context, list.Storage[i])))
                                        return list.Storage[i];

                                return null;
                            }
                            else
                            {
                                throw OperationsHelper.BuildParamException(functionName, 1, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", parameters[1]?.TypeName ?? "null");
                            }
                        }),
                    [new NuaString("find_last")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "find_last";

                            var list = OperationsHelper.TakeListParam(functionName, parameters, 0);
                            OperationsHelper.EnsureParamIndex(functionName, parameters, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", 1);

                            if (parameters[1] is NuaNumber nuaStartIndex)
                            {
                                OperationsHelper.EnsureParamIndex(functionName, parameters, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", 2);
                                if (parameters[2] is NuaNumber nuaCount)
                                {
                                    var nuaMatch = OperationsHelper.TakeFunctionParam(functionName, parameters, 3);

                                    int
                                        startIndex = (int)nuaStartIndex.Value,
                                        count = Math.Min(list.Storage.Count - startIndex, (int)nuaCount.Value);

                                    for (int i = startIndex + count - 1; i >= startIndex; i--)
                                        if (NuaUtilities.ConditionTest(nuaMatch.Invoke(context, list.Storage[i])))
                                            return list.Storage[i];

                                    return null;
                                }
                                else if (parameters[2] is NuaFunction nuaMatch)
                                {
                                    int
                                        startIndex = (int)nuaStartIndex.Value;

                                    for (int i = list.Storage.Count - 1; i >= startIndex; i--)
                                        if (NuaUtilities.ConditionTest(nuaMatch.Invoke(context, list.Storage[i])))
                                            return list.Storage[i];

                                    return null;
                                }
                                else
                                {
                                    throw OperationsHelper.BuildParamException(functionName, 2, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", parameters[2]?.TypeName ?? "null");
                                }

                            }
                            else if (parameters[1] is NuaFunction nuaMatch)
                            {
                                for (int i = list.Storage.Count - 1; i >= 0; i--)
                                    if (NuaUtilities.ConditionTest(nuaMatch.Invoke(context, list.Storage[i])))
                                        return list.Storage[i];

                                return null;
                            }
                            else
                            {
                                throw OperationsHelper.BuildParamException(functionName, 1, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", parameters[1]?.TypeName ?? "null");
                            }
                        }),
                    [new NuaString("find_index")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "find_index";

                            var list = OperationsHelper.TakeListParam(functionName, parameters, 0);
                            OperationsHelper.EnsureParamIndex(functionName, parameters, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", 1);

                            if (parameters[1] is NuaNumber nuaStartIndex)
                            {
                                OperationsHelper.EnsureParamIndex(functionName, parameters, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", 2);
                                if (parameters[2] is NuaNumber nuaCount)
                                {
                                    var nuaMatch = OperationsHelper.TakeFunctionParam(functionName, parameters, 3);

                                    return new NuaNumber(list.Storage.FindIndex((int)nuaStartIndex.Value, (int)nuaCount.Value, v => NuaUtilities.ConditionTest(nuaMatch.Invoke(context, v))));
                                }
                                else if (parameters[2] is NuaFunction nuaMatch)
                                {
                                    return new NuaNumber(list.Storage.FindIndex((int)nuaStartIndex.Value, v => NuaUtilities.ConditionTest(nuaMatch.Invoke(context, v))));
                                }
                                else
                                {
                                throw OperationsHelper.BuildParamException(functionName, 2, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", parameters[2]?.TypeName ?? "null");
                                }
                            }
                            else if (parameters[1] is NuaFunction nuaMatch)
                            {
                                return new NuaNumber(list.Storage.FindIndex(v => NuaUtilities.ConditionTest(nuaMatch.Invoke(context, v))));
                            }
                            else
                            {
                                throw OperationsHelper.BuildParamException(functionName, 1, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", parameters[1]?.TypeName ?? "null");
                            }
                        }),
                    [new NuaString("find_last_index")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "find_last_index";

                            var list = OperationsHelper.TakeListParam(functionName, parameters, 0);
                            OperationsHelper.EnsureParamIndex(functionName, parameters, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", 1);

                            if (parameters[1] is NuaNumber nuaStartIndex)
                            {
                                OperationsHelper.EnsureParamIndex(functionName, parameters, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", 2);
                                if (parameters[2] is NuaNumber nuaCount)
                                {
                                    var nuaMatch = OperationsHelper.TakeFunctionParam(functionName, parameters, 3);

                                    return new NuaNumber(list.Storage.FindLastIndex((int)nuaStartIndex.Value, (int)nuaCount.Value, v => NuaUtilities.ConditionTest(nuaMatch.Invoke(context, v))));
                                }
                                else if (parameters[2] is NuaFunction nuaMatch)
                                {
                                    return new NuaNumber(list.Storage.FindLastIndex((int)nuaStartIndex.Value, v => NuaUtilities.ConditionTest(nuaMatch.Invoke(context, v))));
                                }
                                else
                                {
                                throw OperationsHelper.BuildParamException(functionName, 2, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", parameters[2]?.TypeName ?? "null");
                                }
                            }
                            else if (parameters[1] is NuaFunction nuaMatch)
                            {
                                return new NuaNumber(list.Storage.FindLastIndex(v => NuaUtilities.ConditionTest(nuaMatch.Invoke(context, v))));
                            }
                            else
                            {
                                throw OperationsHelper.BuildParamException(functionName, 1, $"{NuaValue.FunctionTypeName} or {NuaValue.NumberTypeName}", parameters[1]?.TypeName ?? "null");
                            }
                        }),
                    [new NuaString("index_of")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "index_of";

                            var list = OperationsHelper.TakeListParam(functionName, parameters, 0);
                            var item = OperationsHelper.TakeAnyParam(functionName, parameters, 1);

                            if (2 < parameters.Length)
                            {
                                var index = (int)OperationsHelper.TakeNumberParam(functionName, parameters, 2);

                                if (3 < parameters.Length)
                                {
                                    var count = (int)OperationsHelper.TakeNumberParam(functionName, parameters, 2);

                                    return new NuaNumber(list.Storage.IndexOf(item, index, count));
                                }
                                else
                                {
                                    return new NuaNumber(list.Storage.IndexOf(item, index));
                                }
                            }
                            else
                            {
                                return new NuaNumber(list.Storage.IndexOf(item));
                            }
                        }),
                    [new NuaString("remove_at")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            var list = OperationsHelper.TakeListParam("remove_at", parameters, 0);
                            var index = (int)OperationsHelper.TakeNumberParam("remove_at", parameters, 1);

                            if (index >= 0 && index < list.Storage.Count)
                                list.Storage.RemoveAt(index);

                            return list;
                        }),
                    [new NuaString("remove")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            var list = OperationsHelper.TakeListParam("remove_at", parameters, 0);
                            var index = (int)OperationsHelper.TakeNumberParam("remove_at", parameters, 1);

                            if (index >= 0 && index < list.Storage.Count)
                                list.Storage.RemoveAt(index);

                            return list;
                        }),
                }
            };
        }
    }
}
