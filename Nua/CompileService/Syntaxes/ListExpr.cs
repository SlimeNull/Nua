using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ListExpr : ValueExpr
{
    public IEnumerable<Expr> ValueExpressions { get; }

    public ListExpr(IEnumerable<Expr> valueExpressions)
    {
        ValueExpressions = valueExpressions;
    }

    public override NuaValue? Evaluate(NuaContext context)
    {
        NuaList list = new();
        foreach (var value in ValueExpressions)
            list.Storage.Add(value.Evaluate(context));

        return list;
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;

        if (ValueExpressions is not null)
            foreach (var expr in ValueExpressions)
                foreach (var syntax in expr.TreeEnumerate())
                    yield return syntax;
    }

    public override CompiledSyntax Compile()
    {
        List<CompiledSyntax> compiledItems = new();
        foreach (var valueExpr in ValueExpressions)
            compiledItems.Add(valueExpr.Compile());

        NuaList? bufferedValue = null;

        return CompiledSyntax.CreateFromDelegate((context) =>
        {
            if (bufferedValue == null)
            {
                bufferedValue = new();
                foreach (var compiledItem in compiledItems)
                    bufferedValue.Storage.Add(compiledItem.Evaluate(context));
            }

            return bufferedValue;
        });
    }
}
