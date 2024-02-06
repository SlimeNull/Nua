using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ForOfExpr : ForExpr
    {
        public string ValueName { get; }
        public Expr StartExpr { get; }
        public Expr EndExpr { get; }
        public Expr? StepExpr { get; }
        public MultiExpr? BodyExpr { get; }

        public ForOfExpr(string valueName, Expr startExpr, Expr endExpr, Expr? stepExpr, MultiExpr? bodyExpr)
        {
            ValueName = valueName;
            StartExpr = startExpr;
            EndExpr = endExpr;
            StepExpr = stepExpr;
            BodyExpr = bodyExpr;
        }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            var start = StartExpr.Evaluate(context);

            if (start == null)
                throw new NuaEvalException("Start value of 'for-of' statement is null");
            if (start is not NuaNumber startNumber)
                throw new NuaEvalException("Start value of 'for-of' statement not number");

            var end = EndExpr.Evaluate(context);

            if (end == null)
                throw new NuaEvalException("End value of 'for-of' statement is null");
            if (end is not NuaNumber endNumber)
                throw new NuaEvalException("End value of 'for-of' statement not number");

            NuaNumber? stepNumber = null;
            var step = StepExpr?.Evaluate(context) as NuaNumber;

            if (StepExpr != null && stepNumber == null)
                throw new NuaEvalException("Step value of 'for-of' statement not number");

            double startValue = startNumber.Value;
            double endValue = endNumber.Value;
            double stepValue = stepNumber?.Value ?? 1;

            NuaValue? result = null;
            if (endValue >= startValue)
            {
                stepValue = Math.Abs(stepValue);
                for (double value = startValue; value <= endValue; value += stepValue)
                {
                    context.Set(ValueName, new NuaNumber(value));

                    EvalState bodyState = EvalState.None;
                    result = BodyExpr?.Evaluate(context, out bodyState);

                    if (bodyState == EvalState.Continue)
                        continue;
                    else if (bodyState == EvalState.Break)
                        break;
                }
            }
            else
            {
                stepValue = -Math.Abs(stepValue);
                for (double value = startValue; value >= endValue; value += stepValue)
                {
                    context.Set(ValueName, new NuaNumber(value));

                    EvalState bodyState = EvalState.None;
                    result = BodyExpr?.Evaluate(context, out bodyState);

                    if (bodyState == EvalState.Continue)
                        continue;
                    else if (bodyState == EvalState.Break)
                        break;
                }
            }

            state = EvalState.None;
            return result;
        }

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;
            foreach (var syntax in StartExpr.TreeEnumerate())
                yield return syntax;
            foreach (var syntax in EndExpr.TreeEnumerate())
                yield return syntax;

            if (StepExpr is not null)
                foreach (var syntax in StepExpr.TreeEnumerate())
                    yield return syntax;

            if (BodyExpr is not null)
                foreach (var syntax in BodyExpr.TreeEnumerate())
                    yield return syntax;
        }
    }
}
