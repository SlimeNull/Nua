using Nua.Types;

namespace Nua.Stdlib;

public class MathOperations : NuaNativeTable
{
    private MathOperations() { }

    public static MathOperations Create()
    {
        Random _random = new();
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
                [new NuaString("fmod")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("fmod", parameters, 0);
                        double y = OperationsHelper.TakeNumberParam("fmod", parameters, 1);
                        return new NuaNumber(Math.IEEERemainder(x, y));
                    }),
                //[new NuaString("frexp")] = new NuaDelegateFunction(
                //    (context, parameters) =>
                //    {
                //        double x = OperationsHelper.TakeNumberParam("frexp", parameters, 0);
                //        return new NuaNumber(/x/);
                //    }),
                [new NuaString("huge")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        return new NuaNumber(double.PositiveInfinity);
                    }),
                [new NuaString("ldexp")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double m = OperationsHelper.TakeNumberParam("ldexp", parameters, 0);
                        double e = OperationsHelper.TakeNumberParam("ldexp", parameters, 1);
                        return new NuaNumber(m * Math.Pow(2, e));
                    }),
                [new NuaString("log")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("log", parameters, 0);
                        return new NuaNumber(Math.Log(x));
                    }),
                [new NuaString("log10")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("log10", parameters, 0);
                        return new NuaNumber(Math.Log10(x));
                    }),
                //[new NuaString("max")] = new NuaDelegateFunction(
                //    (context, parameters) =>
                //    {
                //        double x = OperationsHelper.TakeNumberParam("max", parameters, 0);
                //        double ... = OperationsHelper.TakeNumberParam("max", parameters, ...);
                //        return new NuaNumber(Math.Log10(x));
                //    }),
                //[new NuaString("min")] = new NuaDelegateFunction(
                //    (context, parameters) =>
                //    {
                //        double x = OperationsHelper.TakeNumberParam("min", parameters, 0);
                //        double ... = OperationsHelper.TakeNumberParam("min", parameters, ...);
                //        return new NuaNumber(Math.Log10(x));
                //    }),
                //[new NuaString("modf")] = new NuaDelegateFunction( // 返回整数和小数部分
                //    (context, parameters) =>
                //    {
                //        double x = OperationsHelper.TakeNumberParam("modf", parameters, 0);
                //        return new NuaNumber(...);
                //    }),
                [new NuaString("pi")] = new NuaNumber(Math.PI),
                [new NuaString("rad")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("rad", parameters, 0);
                        return new NuaNumber(x * Math.PI / 180);
                    }),
                [new NuaString("rand")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        if(parameters.Length == 0)
                        {
                            return new NuaNumber(_random.NextDouble());
                        }
                        else
                        {
                            double n = OperationsHelper.TakeNumberParam("rand", parameters, 0);
                            double m = OperationsHelper.TakeNumberParam("rand", parameters, 1);
                            return new NuaNumber(_random.Next((int)n, (int)(m + 1)));
                        }
                    }),
                [new NuaString("randomseed")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double n = OperationsHelper.TakeNumberParam("randomseed", parameters, 0);
                        _random = new Random((int)n);
                        return null;
                    }),
                [new NuaString("sin")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("sin", parameters, 0);
                        return new NuaNumber(Math.Sin(x));
                    }),
                [new NuaString("sinh")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("sinh", parameters, 0);
                        return new NuaNumber(Math.Sinh(x));
                    }),
                [new NuaString("sqrt")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("sqrt", parameters, 0);
                        return new NuaNumber(Math.Sqrt(x));
                    }),
                [new NuaString("tan")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("tan", parameters, 0);
                        return new NuaNumber(Math.Tan(x));
                    }),
                [new NuaString("tanh")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("tanh", parameters, 0);
                        return new NuaNumber(Math.Tanh(x));
                    }),
            }
        };
    }
}
