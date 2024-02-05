using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class OrTailExpr : Expr
    {
        public OrTailExpr(Expr rightExpr, OrTailExpr? nextTailExpr)
        {
            RightExpr = rightExpr;
            NextTailExpr = nextTailExpr;
        }

        public Expr RightExpr { get; }
        public OrTailExpr? NextTailExpr { get; }

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
    }
}
