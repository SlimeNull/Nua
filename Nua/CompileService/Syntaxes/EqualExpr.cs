using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class EqualExpr : Expr
    {
        public EqualExpr(Expr leftExpr, EqualTailExpr tailExpr)
        {
            LeftExpr = leftExpr;
            TailExpr = tailExpr;
        }

        public Expr LeftExpr { get; }
        public EqualTailExpr TailExpr { get; }

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
