using System.Runtime.CompilerServices;
using Nua.Types;

namespace Nua.Stdlib
{
    public static class OperationsHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception BuildParamException(string functionName, int index, string expect, string got)
        {
            return new NuaEvalException($"bad parameter #{index} to {functionName} ({expect} expected, got {got})");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureParamIndex(string functionName, NuaValue?[] parameters, string expect, int index)
        {
            if (index < 0 || index >= parameters.Length)
                throw BuildParamException(functionName, index, expect, "no value");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NuaValue TakeAnyParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, "any", index);

            if (parameters[index] is not NuaValue value)
                throw BuildParamException(functionName, index, "any", parameters[index]?.TypeName ?? "null");

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TakeBooleanParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, NuaValue.BooleanTypeName, index);

            if (parameters[index] is not NuaBoolean str)
                throw BuildParamException(functionName, index, NuaValue.BooleanTypeName, parameters[index]?.TypeName ?? "null");

            return str.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TakeStringParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, NuaValue.StringTypeName, index);

            if (parameters[index] is not NuaString str)
                throw BuildParamException(functionName, index, NuaValue.StringTypeName, parameters[index]?.TypeName ?? "null");

            return str.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double TakeNumberParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, NuaValue.NumberTypeName, index);

            if (parameters[index] is not NuaNumber number)
                throw BuildParamException(functionName, index, NuaValue.NumberTypeName, parameters[index]?.TypeName ?? "null");

            return number.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NuaFunction TakeFunctionParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, NuaValue.FunctionTypeName, index);

            if (parameters[index] is not NuaFunction function)
                throw BuildParamException(functionName, index, NuaValue.FunctionTypeName, parameters[index]?.TypeName ?? "null");

            return function;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NuaTable TakeTableParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, NuaValue.TableTypeName, index);

            if (parameters[index] is not NuaTable table)
                throw BuildParamException(functionName, index, NuaValue.TableTypeName, parameters[index]?.TypeName ?? "null");

            return table;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NuaList TakeListParam(string functionName, NuaValue?[] parameters, int index)
        {
            EnsureParamIndex(functionName, parameters, NuaValue.ListTypeName, index);

            if (parameters[index] is not NuaList list)
                throw BuildParamException(functionName, index, NuaValue.ListTypeName, parameters[index]?.TypeName ?? "null");

            return list;
        }
    }
}
