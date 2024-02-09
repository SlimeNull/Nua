using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ValueAccessExpr : PrimaryExpr, IAssignableSyntax
{
    public ValueExpr Value { get; }
    public IEnumerable<ValueAccessTailSyntax> Tails { get; }

    public ValueAccessExpr(ValueExpr valueExpr, IEnumerable<ValueAccessTailSyntax> tails)
    {
        Value = valueExpr ?? throw new ArgumentNullException(nameof(valueExpr));
        Tails = tails;
    }

    public override NuaValue? Evaluate(NuaContext context)
    {
        NuaValue? value = Value.Evaluate(context);

        foreach (var tail in Tails)
            value = tail.Evaluate(context, value);

        return value;
    }
    public override CompiledSyntax Compile()
    {
        var result = Value.Compile();

        foreach (var tail in Tails)
            result = tail.Compile(result);

        return result;
    }

    public void Assign(NuaContext context, NuaValue? value)
    {
        NuaValue? valueToAccess = Value.Evaluate(context);
        ValueAccessTailSyntax? lastTail = null;
        foreach (var tail in Tails)
        {
            if (lastTail is not null)
                valueToAccess = lastTail.Evaluate(context, valueToAccess);

            lastTail = tail;
        }

        if (valueToAccess is NuaTable table)
        {
            NuaValue? key;
            if (lastTail is ValueMemberAccessTailSyntax memberAccessTail)
                key = new NuaString(memberAccessTail.Name);
            else if (lastTail is ValueIndexAccessTailSyntax indexAccessTail)
                key = indexAccessTail.IndexExpr.Evaluate(context);
            else
                throw new NuaEvalException("Only Table member, List member or Variable can be assigned");

            if (key != null)
                table.Set(context, key, value);
        }
        else if (valueToAccess is NuaList list)
        {
            NuaValue? index;
            if (lastTail is ValueIndexAccessTailSyntax indexAccessTail)
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

            list.Storage[realIndex] = value;
        }
        else
        {
            throw new NuaEvalException("Unable to access member of non-table value");
        }
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in Value.TreeEnumerate())
            yield return syntax;
        foreach (var tail in Tails)
            foreach (var syntax in tail.TreeEnumerate())
                yield return syntax;
    }
}
