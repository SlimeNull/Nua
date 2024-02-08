using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class QuotedChainExpr : EvaluableSyntax
{
    public QuotedChainExpr(ChainExpr chainExpr)
    {
        ChainExpr = chainExpr;
    }

    public ChainExpr ChainExpr { get; }

    public override NuaValue? Evaluate(NuaContext context) => ChainExpr.Evaluate(context);
    public override CompiledSyntax Compile() => ChainExpr.Compile();

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
        foreach (var syntax in ChainExpr.TreeEnumerate())
            yield return syntax;
    }
}
