using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nua.Types;

namespace Nua.Stdlib
{
    public class TableOperations : StandardModuleTable
    {
        private TableOperations() { }

        public static TableOperations Create()
        {
            return new TableOperations()
            {
                Storage =
                {
                    [new NuaString("contains_key")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "contains_key";

                            var table = OperationsHelper.TakeTableParam(functionName, parameters, 0);
                            var key = OperationsHelper.TakeAnyParam(functionName, parameters, 1);

                            bool isContains = false;
                            if (table is NuaNativeTable nativeTable)
                                isContains = nativeTable.Storage.ContainsKey(key);

                            return new NuaBoolean(isContains);
                        }, "table", "key"),
                    [new NuaString("contains_value")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "contains_value";

                            var table = OperationsHelper.TakeTableParam(functionName, parameters, 0);
                            var value = OperationsHelper.TakeAnyParam(functionName, parameters, 1);

                            bool isContainsValue = false;
                            if (table is NuaNativeTable nativeTable)
                                isContainsValue = nativeTable.Storage.ContainsValue(value);

                            return new NuaBoolean(isContainsValue);
                        }, "table", "value"),
                    [new NuaString("clear")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "clear";

                            var table = OperationsHelper.TakeTableParam(functionName, parameters, 0);

                            if (table is NuaNativeTable nativeTable)
                                nativeTable.Storage.Clear();

                            return table;
                        }),
                    [new NuaString("raw_get")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "raw_get";

                            var table = OperationsHelper.TakeTableParam(functionName, parameters, 0);
                            var key = OperationsHelper.TakeAnyParam(functionName, parameters, 1);

                            if (table is NuaNativeTable nativeTable &&
                                nativeTable.Storage.TryGetValue(key, out var value))
                                return value;
                            
                            return null;
                        }, "table", "key"),
                    [new NuaString("raw_set")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "raw_set";

                            var table = OperationsHelper.TakeTableParam(functionName, parameters, 0);
                            var key = OperationsHelper.TakeAnyParam(functionName, parameters, 1);
                            OperationsHelper.EnsureParamIndex(functionName, parameters, "any", 2);
                            var value = parameters[2];

                            if (table is NuaNativeTable nativeTable)
                            {
                                if (value is not null)
                                    nativeTable.Storage[key] = value;
                                else
                                    nativeTable.Storage.Remove(key);
                            }

                            return null;
                        }, "table", "key", "value"),
                }
            };
        }
    }
}
