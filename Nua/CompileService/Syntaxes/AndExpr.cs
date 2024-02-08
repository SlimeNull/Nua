using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AndExpr : Expr
    {
        public AndExpr(IEnumerable<Expr> segments)
        {
            Segments = segments;
        }


        public IEnumerable<Expr> Segments { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            var segmentsEnumerator = Segments.GetEnumerator();

            if (!segmentsEnumerator.MoveNext())
                return null;

            var result = segmentsEnumerator.Current.Evaluate(context);
            while (EvalUtilities.ConditionTest(result) && segmentsEnumerator.MoveNext())
                result = segmentsEnumerator.Current.Evaluate(context);

            return result;
        }

        public override CompiledSyntax Compile()
        {
            var compiledSegments = Segments
                .Select(segment => segment.Compile())
                .ToList();

            return CompiledSyntax.CreateFromDelegate(context =>
            {
                var compiledSegmentsEnumerator = compiledSegments.GetEnumerator();
                if (!compiledSegmentsEnumerator.MoveNext())
                    return null;

                var result = compiledSegmentsEnumerator.Current.Evaluate(context);
                while (EvalUtilities.ConditionTest(result) && compiledSegmentsEnumerator.MoveNext())
                    result = compiledSegmentsEnumerator.Current.Evaluate(context);

                return result;
            });
        }

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;
            foreach (var segment in Segments)
                foreach (var syntax in segment.TreeEnumerate())
                    yield return syntax;
        }
    }
}
