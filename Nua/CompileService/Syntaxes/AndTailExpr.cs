using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AndTailExpr : Expr
    {
        public Expr RightExpr { get; }
        public AndTailExpr? NextTailExpr { get; }

        public AndTailExpr(Expr rightExpr, AndTailExpr? nextTailExpr)
        {
            RightExpr = rightExpr;
            NextTailExpr = nextTailExpr;
        }

        public override NuaValue? Evaluate(NuaContext context)
        {
            var rightValue = RightExpr.Evaluate(context);

            if (NextTailExpr == null)
                return rightValue;

            if (rightValue is NuaBoolean leftBoolean && leftBoolean.Value)
            {
                return NextTailExpr.Evaluate(context);
            }

            return new NuaBoolean(false);
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out AndTailExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdAnd, ref cursor, out _, out _))
            {
                parseStatus.RequireMoreTokens = required;
                parseStatus.Message = null;
                return false;
            }

            if (!EqualExpr.Match(tokens, true, ref cursor, out parseStatus, out var right))
            {
                if (parseStatus.Message == null)
                    parseStatus.Message = "Require 'equal-expression' after 'and' keyword";

                return false;
            }

            if (!Match(tokens, false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
            {
                parseStatus.RequireMoreTokens = true;
                parseStatus.Message = tailParseStatus.Message;
                return false;
            }

            index = cursor;
            expr = new AndTailExpr(right, nextTail);
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return true;
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
