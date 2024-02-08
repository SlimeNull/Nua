using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class AssignExpr : Expr
{
    public IEnumerable<KeyValuePair<AssignOperation, Expr>> Assignments { get; }
    public Expr TailValue { get; }

    public AssignExpr(IEnumerable<KeyValuePair<AssignOperation, Expr>> Assignments, Expr TailValue)
    {
        this.Assignments = Assignments;
        this.TailValue = TailValue;
    }

    public override NuaValue? Evaluate(NuaContext context)
    {
        var assignmentsReverseEnumerator = Assignments
            .Reverse()
            .GetEnumerator();

        var result = TailValue.Evaluate(context);
        while (assignmentsReverseEnumerator.MoveNext())
        {
            var assignment = assignmentsReverseEnumerator.Current;
            var left = assignment.Value;
            var newValue = assignment.Key switch
            {
                AssignOperation.AddWith => EvalUtilities.EvalPlus(left.Evaluate(context), result),
                AssignOperation.MinWith => EvalUtilities.EvalMinus(left.Evaluate(context), result),
                AssignOperation.Assign or _ => result
            };

            if (left is not IAssignableSyntax assignable)
                throw new NuaEvalException("Target is not assignable");

            assignable.Assign(context, newValue);
            result = newValue;
        }

        return result;
    }

    public override CompiledSyntax Compile()
    {
        var compiledReverseAssignments = Assignments
            .Select(assignment => new KeyValuePair<AssignOperation, (Expr Expr, CompiledSyntax Compiled)>(assignment.Key, (assignment.Value, assignment.Value.Compile())))
            .Reverse()
            .ToList();
        var compiledTailValue = TailValue.Compile();
        return CompiledSyntax.CreateFromDelegate(context =>
        {
            var compiledAssignmentsReverseEnumerator = compiledReverseAssignments
                .GetEnumerator();

            var result = compiledTailValue.Evaluate(context);
            while (compiledAssignmentsReverseEnumerator.MoveNext())
            {
                var assignment = compiledAssignmentsReverseEnumerator.Current;
                var leftExpr = assignment.Value.Expr;
                var compiledLeft= assignment.Value.Compiled;
                var newValue = assignment.Key switch
                {
                    AssignOperation.AddWith => EvalUtilities.EvalPlus(compiledLeft.Evaluate(context), result),
                    AssignOperation.MinWith => EvalUtilities.EvalMinus(compiledLeft.Evaluate(context), result),
                    AssignOperation.Assign or _ => result
                };

                if (leftExpr is not IAssignableSyntax assignable)
                    throw new NuaEvalException("Target is not assignable");

                assignable.Assign(context, newValue);
                result = newValue;
            }

            return result;
        });
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;

        foreach (var assignment in Assignments)
            foreach (var syntax in assignment.Value.TreeEnumerate())
                yield return syntax;

        foreach (var syntax in TailValue.TreeEnumerate())
            yield return syntax;
    }
}
