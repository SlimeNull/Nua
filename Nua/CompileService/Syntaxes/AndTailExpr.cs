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

        public override NuaValue? Eval(NuaContext context)
        {
            var rightValue = Right.Eval(context);

            if (NextTail == null)
                return rightValue;

            if (rightValue is NuaBoolean leftBoolean && leftBoolean.Value)
            {
                return NextTail.Eval(context);
            }

            return new NuaBoolean(false);
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out AndTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.KwdAnd)
                return false;
            cursor++;

            if (!Expr.Match(ExprLevel.Equal, tokens, ref cursor, out var right))
                return false;

            Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new AndTailExpr(right, nextTail);
            return true;
        }
    }
}
