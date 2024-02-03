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

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out AndTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdAnd, ref cursor, out _, out _))
            {
                requireMoreTokens = required;
                message = null;
                return false;
            }

            if (!EqualExpr.Match(tokens, true, ref cursor, out requireMoreTokens, out message, out var right))
            {
                if (message == null)
                    message = "Require 'equal-expression' after 'and' keyword";

                return false;
            }

            if (!Match(tokens, false, ref cursor, out var tailRequireMoreTokens, out var tailMessage, out var nextTail) && tailRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = tailMessage;
                return false;
            }

            index = cursor;
            expr = new AndTailExpr(right, nextTail);
            requireMoreTokens = false;
            message = null;
            return true;
        }
    }
}
