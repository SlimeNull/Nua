using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class OrTailExpr : Expr
    {
        public OrTailExpr(Expr right, OrTailExpr? nextTail)
        {
            Right = right;
            NextTail = nextTail;
        }

        public Expr Right { get; }
        public OrTailExpr? NextTail { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            var rightValue = Right.Evaluate(context);

            if (rightValue == null)
            {
                if (NextTail == null)
                    return new NuaBoolean(false);
                else
                    return NextTail.Evaluate(context);
            }

            if (rightValue is not NuaBoolean rightBoolean)
                return rightValue;

            if (!rightBoolean.Value)
            {
                if (NextTail == null)
                    return new NuaBoolean(false);
                else
                    return NextTail.Evaluate(context);
            }

            return new NuaBoolean(true);
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out OrTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdOr, ref cursor, out _, out _))
            {
                requireMoreTokens = required;
                message = null;
                return false;
            }

            if (!AndExpr.Match(tokens, true, ref cursor, out requireMoreTokens, out message, out var right))
            {
                if (message == null)
                    message = "Expect 'and-expression' after 'or' keyword";

                return false;
            }

            if (!Match(tokens, false, ref cursor, out var tailRequireMoreTokens, out var tailMessage, out var nextTail) && tailRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = tailMessage;
                return false;
            }

            index = cursor;
            expr = new OrTailExpr(right, nextTail);
            requireMoreTokens = false;
            message = null;
            return true;
        }
    }
}
