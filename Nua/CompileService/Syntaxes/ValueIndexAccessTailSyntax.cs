using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ValueIndexAccessTailSyntax : ValueAccessTailSyntax
{
    public Expr IndexExpr { get; }

    public ValueIndexAccessTailSyntax(Expr indexExpr)
    {
        IndexExpr = indexExpr;
    }

    public override NuaValue? Evaluate(NuaContext context, NuaValue? valueToAccess)
    {
        if (valueToAccess == null)
            throw new NuaEvalException("Unable to index a null value");

        NuaValue? result;
        if (valueToAccess is NuaTable table)
        {
            NuaValue? index = IndexExpr.Evaluate(context);

            if (index == null)
                throw new NuaEvalException("Index is null");

            result = table.Get(context, index);
        }
        else if (valueToAccess is NuaList list)
        {
            NuaValue? index = IndexExpr.Evaluate(context);

            if (index == null)
                throw new NuaEvalException("Index is null");
            if (index is not NuaNumber number)
                throw new NuaEvalException("Unable to index a List with non-number value");

            int intIndex = (int)number.Value;
            if (intIndex != number.Value)
            {
                result = null;
            }
            else
            {
                if (intIndex < 0)
                    intIndex = list.Storage.Count + intIndex;
                if (intIndex < 0 || intIndex >= list.Storage.Count)
                    result = null;
                else
                    result = list.Storage[intIndex];
            }
        }
        else if (valueToAccess is NuaString str)
        {
            NuaValue? index = IndexExpr.Evaluate(context);

            if (index == null)
                throw new NuaEvalException("Index is null");
            if (index is not NuaNumber number)
                throw new NuaEvalException("Unable to index a List with non-number value");

            int intIndex = (int)number.Value;
            if (intIndex != number.Value)
            {
                result = null;
            }
            else
            {
                if (intIndex < 0)
                    intIndex = str.Value.Length + intIndex;
                if (intIndex < 0 || intIndex >= str.Value.Length)
                    result = null;
                else
                    result = new NuaString(str.Value.Substring(intIndex, 1));
            }
        }
        else
        {
            throw new NuaEvalException("Only Dictionary, List and String can be indexed");
        }

        return result;
    }
    public override CompiledSyntax Compile(CompiledSyntax compiledValueToAccess)
    {
        CompiledSyntax compiledIndex = IndexExpr.Compile();
        CompiledSyntax result = CompiledSyntax.CreateFromDelegate(context =>
        {
            var valueToAccess = compiledValueToAccess.Evaluate(context);

            if (valueToAccess is NuaTable table)
            {
                NuaValue? index = compiledIndex.Evaluate(context);

                if (index == null)
                    throw new NuaEvalException("Index is null");

                return table.Get(context, index);
            }
            else if (valueToAccess is NuaList list)
            {
                NuaValue? index = compiledIndex.Evaluate(context);

                if (index == null)
                    throw new NuaEvalException("Index is null");
                if (index is not NuaNumber number)
                    throw new NuaEvalException("Unable to index a List with non-number value");

                int intIndex = (int)number.Value;
                if (intIndex != number.Value)
                    return null;
                if (intIndex < 0)
                    intIndex = list.Storage.Count + intIndex;
                if (intIndex < 0 || intIndex >= list.Storage.Count)
                    return null;

                return list.Storage[intIndex];
            }
            else if (valueToAccess is NuaString str)
            {
                NuaValue? index = compiledIndex.Evaluate(context);

                if (index == null)
                    throw new NuaEvalException("Index is null");
                if (index is not NuaNumber number)
                    throw new NuaEvalException("Unable to index a List with non-number value");

                int intIndex = (int)number.Value;
                if (intIndex != number.Value)
                    return null;
                if (intIndex < 0)
                    intIndex = str.Value.Length + intIndex;
                if (intIndex < 0 || intIndex >= str.Value.Length)
                    return null;

                return new NuaString(str.Value.Substring(intIndex, 1));
            }
            else
            {
                throw new NuaEvalException("Only Dictionary, List and String can be indexed");
            }
        });

        return result;
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in IndexExpr.TreeEnumerate())
            yield return syntax;
    }
}
