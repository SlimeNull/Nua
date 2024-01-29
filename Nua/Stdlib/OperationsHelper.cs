using Nua.Types;

namespace Nua.Stdlib
{
    public static class OperationsHelper
    {
        public static Exception BuildParamException(string functionName, int index, string expect, string got)
        {
            return new NuaEvalException($"bad parameter #{index} to {functionName} ({expect} expected, got {got})");
        }

        public static void EnsureParamIndex(string functionName, NuaValue?[] parameters, string expect, int index)
        {
            if (index < 0 || index >= parameters.Length)
                throw BuildParamException(functionName, index, expect, "no value");
        }

        public static NuaValue TakeAnyParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, "any", index);

            if (parameters[index] is not NuaValue value)
                throw BuildParamException(functionName, index, "any", parameters[index]?.TypeName ?? "null");

            return value;
        }

        public static bool TakeBooleanParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, NuaValue.BooleanTypeName, index);

            if (parameters[index] is not NuaBoolean str)
                throw BuildParamException(functionName, index, NuaValue.BooleanTypeName, parameters[index]?.TypeName ?? "null");

            return str.Value;
        }

        public static string TakeStringParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, NuaValue.StringTypeName, index);

            if (parameters[index] is not NuaString str)
                throw BuildParamException(functionName, index, NuaValue.StringTypeName, parameters[index]?.TypeName ?? "null");

            return str.Value;
        }

        public static double TakeNumberParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, NuaValue.NumberTypeName, index);

            if (parameters[index] is not NuaNumber number)
                throw BuildParamException(functionName, index, NuaValue.NumberTypeName, parameters[index]?.TypeName ?? "null");

            return number.Value;
        }

        public static NuaFunction TakeFunctionParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, NuaValue.FunctionTypeName, index);

            if (parameters[index] is not NuaFunction function)
                throw BuildParamException(functionName, index, NuaValue.FunctionTypeName, parameters[index]?.TypeName ?? "null");

            return function;
        }

        public static NuaTable TakeTableParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, NuaValue.TableTypeName, index);

            if (parameters[index] is not NuaTable table)
                throw BuildParamException(functionName, index, NuaValue.TableTypeName, parameters[index]?.TypeName ?? "null");

            return table;
        }

        public static NuaList TakeListParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, NuaValue.ListTypeName, index);

            if (parameters[index] is not NuaList list)
                throw BuildParamException(functionName, index, NuaValue.ListTypeName, parameters[index]?.TypeName ?? "null");

            return list;
        }
    }
}
