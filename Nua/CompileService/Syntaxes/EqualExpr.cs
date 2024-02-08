using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class EqualExpr : Expr
    {
        public Expr Left { get; }
        public IEnumerable<KeyValuePair<EqualOperation, Expr>> Tails { get; }

        public EqualExpr(Expr left, IEnumerable<KeyValuePair<EqualOperation, Expr>> tails)
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
                    EqualOperation.NotEqual => EvalUtilities.EvalNotEqual(result, tail.Value.Evaluate(context)),
                    EqualOperation.Equal or _ => EvalUtilities.EvalEqual(result, tail.Value.Evaluate(context)),
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
                    EqualOperation.NotEqual => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalNotEqual(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
                    EqualOperation.Equal or _ => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalEqual(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
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
