using Nua.Types;

namespace Nua.Stdlib
{
    public class MathOperations : NuaNativeTable
    {
        private MathOperations() { }

        public static MathOperations Create()
        {
            return new MathOperations()
            {
                Storage =
                {
                    [new NuaString("abs")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            double x = OperationsHelper.TakeNumberParam("abs", parameters, 0);
                            return new NuaNumber(Math.Abs(x));
                        }),
                    [new NuaString("acos")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            double x = OperationsHelper.TakeNumberParam("acos", parameters, 0);
                            return new NuaNumber(Math.Acos(x));
                        }),
                    [new NuaString("asin")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            double x = OperationsHelper.TakeNumberParam("asin", parameters, 0);
                            return new NuaNumber(Math.Asin(x));
                        }),
                    [new NuaString("atan")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            double x = OperationsHelper.TakeNumberParam("atan", parameters, 0);
                            return new NuaNumber(Math.Atan(x));
                        }),
                    [new NuaString("atan2")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            double y = OperationsHelper.TakeNumberParam("atan2", parameters, 0);
                            double x = OperationsHelper.TakeNumberParam("atan2", parameters, 1);
                            return new NuaNumber(Math.Atan(y/x));
                        }),
                    [new NuaString("ceil")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            double x = OperationsHelper.TakeNumberParam("ceil", parameters, 0);
                            return new NuaNumber(Math.Ceiling(x));
                        }),
                    [new NuaString("cos")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            double x = OperationsHelper.TakeNumberParam("cos", parameters, 0);
                            return new NuaNumber(Math.Cos(x));
                        }),
                    [new NuaString("cosh")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            double x = OperationsHelper.TakeNumberParam("cosh", parameters, 0);
                            return new NuaNumber(Math.Cosh(x));
                        }),
                    [new NuaString("deg")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            double x = OperationsHelper.TakeNumberParam("deg", parameters, 0);
                            return new NuaNumber(180 * x / Math.PI);
                        }),
                    [new NuaString("exp")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            double x = OperationsHelper.TakeNumberParam("exp", parameters, 0);
                            return new NuaNumber(Math.Exp(x));
                        }),
                    [new NuaString("floor")] = new NuaDelegateFunction(
                        (context, parameters) =>
                        {
                            double x = OperationsHelper.TakeNumberParam("floor", parameters, 0);
                            return new NuaNumber(Math.Floor(x));
                        }),
                }
            };
    }
}
}
