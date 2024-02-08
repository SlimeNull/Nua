using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

/// <summary>
/// xxx: expr,
/// "xxx": expr
/// </summary>
public class TableMemberSyntax : Expr
{
    public TableMemberSyntax(Expr keyExpr, Expr valueExpr)
    {
        KeyExpr = keyExpr;
        ValueExpr = valueExpr;
    }

    public Expr KeyExpr { get; }
    public Expr ValueExpr { get; }

    public override NuaValue? Evaluate(NuaContext context) => ValueExpr.Evaluate(context);

    public override CompiledSyntax Compile() => ValueExpr.Compile();

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in KeyExpr.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in ValueExpr.TreeEnumerate())
            yield return syntax;
    }
}
