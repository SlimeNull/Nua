using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class TableExpr : ValueExpr
{
    public IReadOnlyList<TableMemberSyntax> MemberExpressions { get; }

    public TableExpr(IEnumerable<TableMemberSyntax> memberExpressions)
    {
        MemberExpressions = memberExpressions
            .ToList()
            .AsReadOnly();
    }

    public override NuaValue? Evaluate(NuaContext context)
    {
        var table = new NuaNativeTable();

        foreach (var member in MemberExpressions)
        {
            table.Set(member.KeyExpr.Evaluate(context)!, member.ValueExpr.Evaluate(context));
        }

        return table;
    }

    public override CompiledSyntax Compile()
    {
        NuaValue? bufferedValue = null;

        List<(CompiledSyntax Key, CompiledSyntax Value)> compiledMembers = new(MemberExpressions.Count);
        foreach (var member in MemberExpressions)
            compiledMembers.Add((member.KeyExpr.Compile(), member.ValueExpr.Compile()));

        return CompiledSyntax.CreateFromDelegate(context =>
        {
            if (bufferedValue == null)
            {
                var table = new NuaNativeTable();

                foreach (var member in compiledMembers)
                {
                    table.Set(member.Key.Evaluate(context)!, member.Value.Evaluate(context));
                }

                bufferedValue = table;
            }

            return bufferedValue;
        });
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;

        foreach (var expr in MemberExpressions)
            foreach (var syntax in expr.TreeEnumerate())
                yield return syntax;
    }
}
