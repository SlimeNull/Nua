using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ElseSyntax : Syntax
{
    public MultiExpr? BodyExpr { get; }

    public ElseSyntax(MultiExpr? bodyExpr)
    {
        BodyExpr = bodyExpr;
    }


    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;

        if (BodyExpr is not null)
            foreach (var syntax in BodyExpr.TreeEnumerate())
                yield return syntax;
    }
}
