using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AndExpr : Expr
    {
        public AndExpr(Expr leftExpr, AndTailExpr tailExpr)
        {
            LeftExpr = leftExpr;
            TailExpr = tailExpr;
        }

        public Expr LeftExpr { get; }
        public AndTailExpr TailExpr { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            var leftValue = LeftExpr.Evaluate(context);

            if (leftValue is NuaBoolean leftBoolean && leftBoolean.Value)
                return TailExpr.Evaluate(context);

            return new NuaBoolean(false);
        }

        public override CompiledSyntax Compile()
        {
            var compiledLeft = LeftExpr.Compile();
            var compiledTail = TailExpr.Compile();

            return CompiledSyntax.CreateFromDelegate((context) =>
            {
                var right = compiledLeft.Evaluate(context);
                if (EvalUtilities.ConditionTest(right))
                    return compiledTail.Evaluate(context);
                else
                    return right;
            });
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
