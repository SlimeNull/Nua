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
                    [new NuaString("add")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "add";

                            var table = OperationsHelper.TakeTableParam(functionName, parameters, 0);
                            var key = OperationsHelper.TakeAnyParam(functionName, parameters, 1);
                            var value = OperationsHelper.TakeAnyParam(functionName, parameters, 2);

                            if (table is NuaNativeTable nativeTable)
                                nativeTable.Storage.Add(key, value);
                            
                            return table;
                        }, "key", "value"),
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
                        }, "key"),
                    [new NuaString("remove")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "remove";

                            var table = OperationsHelper.TakeTableParam(functionName, parameters, 0);
                            var key = OperationsHelper.TakeAnyParam(functionName, parameters, 1);

                            bool isRemove = false;
                            if (table is NuaNativeTable nativeTable)
                                isRemove = nativeTable.Storage.Remove(key);

                            return new NuaBoolean(isRemove);
                        }, "key"),
                    [new NuaString("clear")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "clear";

                            var table = OperationsHelper.TakeTableParam(functionName, parameters, 0);

                            if (table is NuaNativeTable nativeTable)
                                nativeTable.Storage.Clear();

                            return table;
                        }),
                    [new NuaString("contains_value")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "contains_value";

                            var table = OperationsHelper.TakeTableParam(functionName, parameters, 0);
                            var key = OperationsHelper.TakeAnyParam(functionName, parameters, 1);

                            bool isContainsValue = false;
                            if (table is NuaNativeTable nativeTable)
                                isContainsValue = nativeTable.Storage.ContainsValue(key);

                            return new NuaBoolean(isContainsValue);
                        }, "key"),
                }
            };
        }
    }
}
