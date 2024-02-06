using Nua.Types;

namespace Nua.Stdlib
{
    public class CoreOperations : StandardModuleTable
    {
        private CoreOperations() { }

        public static CoreOperations Create()
        {
            return new CoreOperations()
            {
                Storage =
                {
                    [new NuaString("to_string")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "to_string";

                            if (parameters.Length == 0)
                                return new NuaString("null");

                            var value = OperationsHelper.TakeAnyParam(functionName, parameters, 0);

                            return new NuaString(value.ToString() ?? string.Empty);
                        }),
                    [new NuaString("to_number")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "to_string";

                            if (parameters.Length == 0)
                                return null;

                            var value = OperationsHelper.TakeAnyParam(functionName, parameters, 0);

                            if (value is not NuaString strValue)
                                return null;

                            if (!double.TryParse(strValue.Value, out var number))
                                return null;

                            return new NuaNumber(number);
                        }),
                    [new NuaString("get_type")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            const string functionName = "get_type";

                            OperationsHelper.EnsureParamIndex(functionName, parameters, "any", 0);

                            var value = parameters[0];
                            if (value is null)
                                return new NuaString("null");
                            else
                                return new NuaString(value.TypeName);
                        }),
                }
            };
        }
    }
}
