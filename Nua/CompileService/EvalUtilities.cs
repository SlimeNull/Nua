using System.Runtime.CompilerServices;
using Nua.Types;

namespace Nua.CompileService;

public static class EvalUtilities
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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


    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalMultiply(NuaValue? left, NuaValue? right)
    {
        if (left is null || right is null)
            throw new NuaEvalException("Unable to plus a null value");

        if (left is NuaNumber leftNumber &&
            right is NuaNumber rightNumber)
            return new NuaNumber(leftNumber.Value * rightNumber.Value);

        throw new NuaEvalException($"Unable to minus {left.TypeName} with {right.TypeName}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalDivide(NuaValue? left, NuaValue? right)
    {
        if (left is null || right is null)
            throw new NuaEvalException("Unable to plus a null value");

        if (left is NuaNumber leftNumber &&
            right is NuaNumber rightNumber)
            return new NuaNumber(leftNumber.Value / rightNumber.Value);

        throw new NuaEvalException($"Unable to minus {left.TypeName} with {right.TypeName}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalPower(NuaValue? left, NuaValue? right)
    {
        if (left is null || right is null)
            throw new NuaEvalException("Unable to plus a null value");

        if (left is NuaNumber leftNumber &&
            right is NuaNumber rightNumber)
            return new NuaNumber(Math.Pow(leftNumber.Value, rightNumber.Value));

        throw new NuaEvalException($"Unable to minus {left.TypeName} with {right.TypeName}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalMod(NuaValue? left, NuaValue? right)
    {
        if (left is null || right is null)
            throw new NuaEvalException("Unable to plus a null value");

        if (left is NuaNumber leftNumber &&
            right is NuaNumber rightNumber)
            return new NuaNumber(leftNumber.Value % rightNumber.Value);

        throw new NuaEvalException($"Unable to minus {left.TypeName} with {right.TypeName}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalDivideInt(NuaValue? left, NuaValue? right)
    {
        if (left is null || right is null)
            throw new NuaEvalException("Unable to plus a null value");

        if (left is NuaNumber leftNumber &&
            right is NuaNumber rightNumber)
            return new NuaNumber(Math.Floor(leftNumber.Value / rightNumber.Value));

        throw new NuaEvalException($"Unable to minus {left.TypeName} with {right.TypeName}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalAnd(NuaValue? left, NuaValue? right)
    {
        if (ConditionTest(left))
            return right;
        else
            return left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalOr(NuaValue? left, NuaValue? right)
    {
        if (!ConditionTest(left))
            return right;
        else
            return left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalLessThan(NuaValue? left, NuaValue? right)
    {
        if (left is not NuaNumber leftNumber)
            throw new NuaEvalException("Unable to compare on a non-number value");
        if (right is not NuaNumber rightNumber)
            throw new NuaEvalException("Unable to compare on a non-number value");

        return new NuaBoolean(leftNumber.Value < rightNumber.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalGreaterThan(NuaValue? left, NuaValue? right)
    {
        if (left is not NuaNumber leftNumber)
            throw new NuaEvalException("Unable to compare on a non-number value");
        if (right is not NuaNumber rightNumber)
            throw new NuaEvalException("Unable to compare on a non-number value");

        return new NuaBoolean(leftNumber.Value > rightNumber.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalLessEqual(NuaValue? left, NuaValue? right)
    {
        if (left is not NuaNumber leftNumber)
            throw new NuaEvalException("Unable to compare on a non-number value");
        if (right is not NuaNumber rightNumber)
            throw new NuaEvalException("Unable to compare on a non-number value");

        return new NuaBoolean(leftNumber.Value <= rightNumber.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalGreaterEqual(NuaValue? left, NuaValue? right)
    {
        if (left is not NuaNumber leftNumber)
            throw new NuaEvalException("Unable to compare on a non-number value");
        if (right is not NuaNumber rightNumber)
            throw new NuaEvalException("Unable to compare on a non-number value");

        return new NuaBoolean(leftNumber.Value >= rightNumber.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalEqual(NuaValue? left, NuaValue? right)
    {
        return new NuaBoolean(Object.Equals(left, right));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static NuaValue? EvalNotEqual(NuaValue? left, NuaValue? right)
    {
        return new NuaBoolean(!Object.Equals(left, right));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool ConditionTest(NuaValue? value)
    {
        if (value == null)
            return false;
        if (value is not NuaBoolean boolean)
            return true;
        else
            return boolean.Value;
    }
}
