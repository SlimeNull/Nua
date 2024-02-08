using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class CompareExpr : Expr
    {
        public Expr Left { get; }
        public IEnumerable<KeyValuePair<CompareOperation, Expr>> Tails { get; }

        public CompareExpr(Expr left, IEnumerable<KeyValuePair<CompareOperation, Expr>> tails)
        {
            Left = left;
            Tails = tails;
        }

        public override NuaValue? Evaluate(NuaContext context)
        {
            var result = Left.Evaluate(context);

            foreach (var tail in Tails)
                result = tail.Key switch
                {
                    CompareOperation.GreaterThan => EvalUtilities.EvalGreaterThan(result, tail.Value.Evaluate(context)),
                    CompareOperation.LessThan => EvalUtilities.EvalLessThan(result, tail.Value.Evaluate(context)),
                    CompareOperation.GreaterEqual => EvalUtilities.EvalGreaterEqual(result, tail.Value.Evaluate(context)),
                    CompareOperation.LessEqual or _ => EvalUtilities.EvalLessEqual(result, tail.Value.Evaluate(context)),
                };

            return result;
        }

        public override CompiledSyntax Compile()
        {
            var result = Left.Compile();

            foreach (var tail in Tails)
            {
                var compiledLeft = result;
                var compiledRight = tail.Value.Compile();
                result = tail.Key switch
                {
                    CompareOperation.GreaterThan => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalGreaterThan(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
                    CompareOperation.LessThan => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalLessThan(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
                    CompareOperation.GreaterEqual => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalGreaterEqual(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
                    CompareOperation.LessEqual or _ => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalLessEqual(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
                };
            }

            return result;
        }

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;
            foreach (var syntax in Left.TreeEnumerate())
                yield return syntax;
            foreach (var tail in Tails)
                foreach (var syntax in tail.Value.TreeEnumerate())
                    yield return syntax;
        }
    }
}
