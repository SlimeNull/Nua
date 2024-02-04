using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AndTailExpr : Expr
    {
        public AndTailExpr(Expr right, AndTailExpr? nextTail)
        {
            Right = right;
            NextTail = nextTail;
        }

        public Expr Right { get; }
        public AndTailExpr? NextTail { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            var rightValue = Right.Evaluate(context);

            if (NextTail == null)
                return rightValue;

            if (rightValue is NuaBoolean leftBoolean && leftBoolean.Value)
            {
                return NextTail.Evaluate(context);
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
    }
}
