using Nua.Types;

namespace Nua.Stdlib
{
    public static class OperationsHelper
    {
        public static Exception BuildParamException(string functionName, int index, string expect, string got)
        {
            return new NuaEvalException($"bad parameter #{index} to {functionName} ({expect} expected, got {got})");
        }

        public static void EnsureParamInBound(string functionName, NuaValue?[] parameters, int index)
        {
            if (index < 0 || index >= parameters.Length)
                throw BuildParamException(functionName, index, NuaValue.BooleanTypeName, "no value");
        }

        public static bool TakeBooleanParam(string functionName, NuaValue?[] parameters, int index)
        {
            if (index < 0 || index >= parameters.Length)
                throw BuildParamException(functionName, index, NuaValue.BooleanTypeName, "no value");
            if (parameters[index] is not NuaBoolean str)
                throw BuildParamException(functionName, index, NuaValue.BooleanTypeName, parameters[index]?.TypeName ?? "null");

            return str.Value;
        }

        public static string TakeStringParam(string functionName, NuaValue?[] parameters, int index)
        {
            if (index < 0 || index >= parameters.Length)
                throw BuildParamException(functionName, index, NuaValue.StringTypeName, "no value");
            if (parameters[index] is not NuaString str)
                throw BuildParamException(functionName, index, NuaValue.StringTypeName, parameters[index]?.TypeName ?? "null");

            return str.Value;
        }

        public static double TakeNumberParam(string functionName, NuaValue?[] parameters, int index)
        {
            if (index < 0 || index >= parameters.Length)
                throw BuildParamException(functionName, index, NuaValue.NumberTypeName, "no value");
            if (parameters[index] is not NuaNumber number)
                throw BuildParamException(functionName, index, NuaValue.NumberTypeName, parameters[index]?.TypeName ?? "null");

            return number.Value;
        }

        public static NuaTable TakeTableParam(string functionName, NuaValue?[] parameters, int index)
        {
            if (index < 0 || index >= parameters.Length)
                throw BuildParamException(functionName, index, NuaValue.TableTypeName, "no value");
            if (parameters[index] is not NuaTable table)
                throw BuildParamException(functionName, index, NuaValue.TableTypeName, parameters[index]?.TypeName ?? "null");

            return table;
        }

        public static NuaList TakeListParam(string functionName, NuaValue?[] parameters, int index)
        {
            if (index < 0 || index >= parameters.Length)
                throw BuildParamException(functionName, index, NuaValue.ListTypeName, "no value");
            if (parameters[index] is not NuaList list)
                throw BuildParamException(functionName, index, NuaValue.ListTypeName, parameters[index]?.TypeName ?? "null");

            return list;
        }
    }
}
