using Nua.Types;

namespace Nua.Stdlib;

public class MathOperations : StandardModuleTable
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
                    }, "x"),
                [new NuaString("acos")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("acos", parameters, 0);
                        return new NuaNumber(Math.Acos(x));
                    }, "x"),
                [new NuaString("asin")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("asin", parameters, 0);
                        return new NuaNumber(Math.Asin(x));
                    }, "x"),
                [new NuaString("atan")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("atan", parameters, 0);
                        return new NuaNumber(Math.Atan(x));
                    }, "x"),
                [new NuaString("atan2")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double y = OperationsHelper.TakeNumberParam("atan2", parameters, 0);
                        double x = OperationsHelper.TakeNumberParam("atan2", parameters, 1);
                        return new NuaNumber(Math.Atan(y/x));
                    }, "y", "x"),
                [new NuaString("ceil")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("ceil", parameters, 0);
                        return new NuaNumber(Math.Ceiling(x));
                    }, "x"),
                [new NuaString("cos")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("cos", parameters, 0);
                        return new NuaNumber(Math.Cos(x));
                    }, "x"),
                [new NuaString("cosh")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("cosh", parameters, 0);
                        return new NuaNumber(Math.Cosh(x));
                    }, "x"),
                [new NuaString("deg")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("deg", parameters, 0);
                        return new NuaNumber(180 * x / Math.PI);
                    }, "x"),
                [new NuaString("exp")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("exp", parameters, 0);
                        return new NuaNumber(Math.Exp(x));
                    }, "x"),
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
                    }, "x", "y"),
                //[new NuaString("frexp")] = new NuaDelegateFunction(
                //    (context, parameters) =>
                //    {
                //        double x = OperationsHelper.TakeNumberParam("frexp", parameters, 0);
                //        return new NuaNumber(/x/);
                //    }),
                [new NuaString("huge")] = new NuaNumber(double.PositiveInfinity),
                [new NuaString("ldexp")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double m = OperationsHelper.TakeNumberParam("ldexp", parameters, 0);
                        double e = OperationsHelper.TakeNumberParam("ldexp", parameters, 1);
                        return new NuaNumber(m * Math.Pow(2, e));
                    }, "m", "e"),
                [new NuaString("log")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("log", parameters, 0);
                        return new NuaNumber(Math.Log(x));
                    }, "x"),
                [new NuaString("log10")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("log10", parameters, 0);
                        return new NuaNumber(Math.Log10(x));
                    }, "x"),
                [new NuaString("max")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        const string functionName = "max";

                        double max = OperationsHelper.TakeNumberParam(functionName, parameters, 0);
                        for (int i = 1; i < parameters.Length; i++)
                        {
                            var current = OperationsHelper.TakeNumberParam(functionName, parameters, i);
                            if (current > max)
                                max = current;
                        }

                        return new NuaNumber(max);
                    }, "value", "..."),
                [new NuaString("min")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        const string functionName = "min";

                        double min = OperationsHelper.TakeNumberParam(functionName, parameters, 0);
                        for (int i = 1; i < parameters.Length; i++)
                        {
                            var current = OperationsHelper.TakeNumberParam(functionName, parameters, i);
                            if (current < min)
                                min = current;
                        }

                        return new NuaNumber(min);
                    }, "value", "..."),
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
                    }, "x"),
                [new NuaString("random")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        const string functionName = "random";

                        if(parameters.Length == 0)
                        {
                            return new NuaNumber(_random.NextDouble());
                        }
                        else
                        {
                            int min = 1;
                            int max = 1;

                            if (0 < parameters.Length)
                                max = (int)OperationsHelper.TakeNumberParam(functionName, parameters, 0);
                            if (1 < parameters.Length)
                            {
                                (min, max) = (max, (int)OperationsHelper.TakeNumberParam(functionName, parameters, 1));
                            }

                            double diff = max - min;

                            return new NuaNumber(_random.Next(min, max + 1));
                        }
                    }, "[m [", "n]]"),
                [new NuaString("randomseed")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double n = OperationsHelper.TakeNumberParam("randomseed", parameters, 0);
                        _random = new Random((int)n);
                        return null;
                    }, "n"),
                [new NuaString("sin")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("sin", parameters, 0);
                        return new NuaNumber(Math.Sin(x));
                    }, "x"),
                [new NuaString("sinh")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("sinh", parameters, 0);
                        return new NuaNumber(Math.Sinh(x));
                    }, "x"),
                [new NuaString("sqrt")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("sqrt", parameters, 0);
                        return new NuaNumber(Math.Sqrt(x));
                    }, "x"),
                [new NuaString("tan")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("tan", parameters, 0);
                        return new NuaNumber(Math.Tan(x));
                    }, "x"),
                [new NuaString("tanh")] = new NuaDelegateFunction(
                    (context, parameters) =>
                    {
                        double x = OperationsHelper.TakeNumberParam("tanh", parameters, 0);
                        return new NuaNumber(Math.Tanh(x));
                    }, "x"),
            }
        };
    }
}
