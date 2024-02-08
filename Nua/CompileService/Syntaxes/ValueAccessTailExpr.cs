using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public abstract class ValueAccessTailExpr : Syntax
{
    public ValueAccessTailExpr? NextTailExpr { get; }

    public ValueAccessTailExpr(ValueAccessTailExpr? nextTailExpr)
    {
        NextTailExpr = nextTailExpr;
    }

    public NuaValue? Evaluate(NuaContext context, Expr expr)
    {
        return Evaluate(context, expr.Evaluate(context));
    }

    public CompiledSyntax Compile(Expr valueToAccessExpr)
    {
        return Compile(valueToAccessExpr.Compile());
    }

    public void SetMemberValue(NuaContext context, NuaValue valueToAccess, NuaValue? newMemberValue)
    {
        if (NextTailExpr != null)
        {
            var value = Evaluate(context, valueToAccess);

            if (value == null)
                throw new NuaEvalException("Unable to access member of null value");

            NextTailExpr.SetMemberValue(context, value, newMemberValue);
        }

        if (valueToAccess is NuaTable table)
        {
            NuaValue? key;
            if (this is ValueMemberAccessTailExpr memberAccessTail)
                key = new NuaString(memberAccessTail.Name);
            else if (this is ValueIndexAccessTailExpr indexAccessTail)
                key = indexAccessTail.IndexExpr.Evaluate(context);
            else
                throw new NuaEvalException("Only Table member, List member or Variable can be assigned");

            if (key != null)
                table.Set(key, newMemberValue);
        }
        else if (valueToAccess is NuaList list)
        {
            NuaValue? index;
            if (this is ValueIndexAccessTailExpr indexAccessTail)
                index = indexAccessTail.IndexExpr.Evaluate(context);
            else
                throw new NuaEvalException("Only Table member, List member or Variable can be assigned");

            if (index is not NuaNumber indexNumber)
                throw new NuaEvalException("List index is not number");

            int realIndex = (int)indexNumber.Value;
            if (realIndex < 0)
            {
                realIndex = list.Storage.Count + realIndex;
                if (realIndex < 0)
                    throw new NuaEvalException("List index is out of range");
            }

            list.Storage.EnsureCapacity(realIndex + 1);
            while (list.Storage.Count <= realIndex)
                list.Storage.Add(null);

            list.Storage[realIndex] = newMemberValue;
        }
        else
        {
            throw new NuaEvalException("Unable to access member of non-table value");
        }
    }

    public void SetMemberValue(NuaContext context, Expr expr, NuaValue? newMemberValue)
    {
        var valueToAccess = expr.Evaluate(context);

        if (valueToAccess == null)
            throw new NuaEvalException("Unable to access member of null value");

        SetMemberValue(context, valueToAccess, newMemberValue);
    }

    public void SetMemberValue(NuaContext context, Expr expr, Expr newMemberValueExpr)
    {

        var newMemberValue = newMemberValueExpr.Evaluate(context);

        SetMemberValue(context, expr, newMemberValue);
    }

    public abstract NuaValue? Evaluate(NuaContext context, NuaValue? valueToAccess);
    public abstract CompiledSyntax Compile(CompiledSyntax compiledValueToAccess);

}
