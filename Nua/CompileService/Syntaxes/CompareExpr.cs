using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class CompareExpr : Expr
    {
        public Expr LeftExpr { get; }
        public CompareTailSyntax TailExpr { get; }

        public CompareExpr(Expr leftExpr, CompareTailSyntax tailExpr)
        {
            LeftExpr = leftExpr;
            TailExpr = tailExpr;
        }

        public override NuaValue? Evaluate(NuaContext context)
        {
            return TailExpr.Evaluate(context, LeftExpr);
        }

        public override CompiledSyntax Compile()
        {
            return TailExpr.Compile(LeftExpr);
        }

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;
            foreach (var syntax in LeftExpr.TreeEnumerate())
                yield return syntax;
            foreach (var syntax in TailExpr.TreeEnumerate())
                yield return syntax;
        }
    }
}
