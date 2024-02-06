using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class OrTailExpr : Expr
    {
        public Expr RightExpr { get; }
        public OrTailExpr? NextTailExpr { get; }

        public OrTailExpr(Expr rightExpr, OrTailExpr? nextTailExpr)
        {
            RightExpr = rightExpr;
            NextTailExpr = nextTailExpr;
        }

        public override NuaValue? Evaluate(NuaContext context)
        {
            var rightValue = RightExpr.Evaluate(context);

            if (rightValue == null)
            {
                if (NextTailExpr == null)
                    return new NuaBoolean(false);
                else
                    return NextTailExpr.Evaluate(context);
            }

            if (rightValue is not NuaBoolean rightBoolean)
                return rightValue;

            if (!rightBoolean.Value)
            {
                if (NextTailExpr == null)
                    return new NuaBoolean(false);
                else
                    return NextTailExpr.Evaluate(context);
            }

            return new NuaBoolean(true);
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out OrTailExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdOr, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = required;
                parseStatus.Message = null;
                return false;
            }

            if (!AndExpr.Match(tokens, true, ref cursor, out parseStatus, out var right))
            {
                parseStatus.Intercept = true;
                if (parseStatus.Message == null)
                    parseStatus.Message = "Expect 'and-expression' after 'or' keyword";

                return false;
            }

            if (!Match(tokens, false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = new OrTailExpr(right, nextTail);
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return true;
        }

        public override CompiledSyntax Compile()
        {
            var compiledRight = RightExpr.Compile();

            if (NextTailExpr == null)
                return compiledRight;

            var compiledNextTail = NextTailExpr.Compile();

            return CompiledSyntax.CreateFromDelegate((context) =>
            {
                var right = compiledRight.Evaluate(context);
                if (EvalUtilities.ConditionTest(right))
                    return right;
                else
                    return compiledNextTail.Evaluate(context);
            });
        }

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;
            foreach (var syntax in RightExpr.TreeEnumerate())
                yield return syntax;

            if (NextTailExpr is not null)
                foreach (var syntax in NextTailExpr.TreeEnumerate())
                    yield return syntax;
        }
    }
}
