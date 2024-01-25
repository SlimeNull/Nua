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

        public override NuaValue? Eval(NuaContext context)
        {
            var rightValue = Right.Eval(context);

            if (rightValue == null)
            {
                if (NextTail == null)
                    return new NuaBoolean(false);
                else
                    return NextTail.Eval(context);
            }

            if (rightValue is not NuaBoolean rightBoolean)
                return rightValue;

            if (!rightBoolean.Value)
            {
                if (NextTail == null)
                    return new NuaBoolean(false);
                else
                    return NextTail.Eval(context);
            }

            return new NuaBoolean(true);
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out OrTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.KwdOr)
                return false;
            cursor++;

            if (!Expr.Match(ExprLevel.And, tokens, ref cursor, out var right))
                return false;

            Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new OrTailExpr(right, nextTail);
            return true;
        }
    }
}
