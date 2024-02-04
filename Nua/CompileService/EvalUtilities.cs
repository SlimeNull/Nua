using Nua.Types;

namespace Nua.CompileService
{
    public static class EvalUtilities
    {


        public static NuaValue? EvalPlus(NuaValue? left, NuaValue? right)
        {
            if (left is null || right is null)
                throw new NuaEvalException("Unable to plus a null value");

            if (left is NuaNumber leftNumber &&
                right is NuaNumber rightNumber)
                return new NuaNumber(leftNumber.Value + rightNumber.Value);
            if (left is NuaString leftString &&
                right is NuaString rightString)
                return new NuaString(leftString.Value + rightString.Value);

            throw new NuaEvalException($"Unable to plus {left.TypeName} with {right.TypeName}");
        }

        public static NuaValue? EvalMinus(NuaValue? left, NuaValue? right)
        {
            if (left is null || right is null)
                throw new NuaEvalException("Unable to plus a null value");

            if (left is NuaNumber leftNumber &&
                right is NuaNumber rightNumber)
                return new NuaNumber(leftNumber.Value - rightNumber.Value);
            if (left is NuaString leftString &&
                right is NuaString rightString)
                return new NuaString(leftString.Value.Replace(rightString.Value, null));

            throw new NuaEvalException($"Unable to minus {left.TypeName} with {right.TypeName}");
        }


        public static NuaValue? EvalMultiply(NuaValue? left, NuaValue? right)
        {
            if (left is null || right is null)
                throw new NuaEvalException("Unable to plus a null value");

            if (left is NuaNumber leftNumber &&
                right is NuaNumber rightNumber)
                return new NuaNumber(leftNumber.Value * rightNumber.Value);

            throw new NuaEvalException($"Unable to minus {left.TypeName} with {right.TypeName}");
        }

        public static NuaValue? EvalDivide(NuaValue? left, NuaValue? right)
        {
            if (left is null || right is null)
                throw new NuaEvalException("Unable to plus a null value");

            if (left is NuaNumber leftNumber &&
                right is NuaNumber rightNumber)
                return new NuaNumber(leftNumber.Value / rightNumber.Value);

            throw new NuaEvalException($"Unable to minus {left.TypeName} with {right.TypeName}");
        }

        public static NuaValue? EvalPower(NuaValue? left, NuaValue? right)
        {
            if (left is null || right is null)
                throw new NuaEvalException("Unable to plus a null value");

            if (left is NuaNumber leftNumber &&
                right is NuaNumber rightNumber)
                return new NuaNumber(Math.Pow(leftNumber.Value, rightNumber.Value));

            throw new NuaEvalException($"Unable to minus {left.TypeName} with {right.TypeName}");
        }

        public static NuaValue? EvalMod(NuaValue? left, NuaValue? right)
        {
            if (left is null || right is null)
                throw new NuaEvalException("Unable to plus a null value");

            if (left is NuaNumber leftNumber &&
                right is NuaNumber rightNumber)
                return new NuaNumber(leftNumber.Value % rightNumber.Value);

            throw new NuaEvalException($"Unable to minus {left.TypeName} with {right.TypeName}");
        }

        public static NuaValue? EvalDivideInt(NuaValue? left, NuaValue? right)
        {
            if (left is null || right is null)
                throw new NuaEvalException("Unable to plus a null value");

            if (left is NuaNumber leftNumber &&
                right is NuaNumber rightNumber)
                return new NuaNumber(Math.Floor(leftNumber.Value / rightNumber.Value));

            throw new NuaEvalException($"Unable to minus {left.TypeName} with {right.TypeName}");
        }
    }
}
