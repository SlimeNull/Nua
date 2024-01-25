using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class EqualTailExpr : Expr
    {
        public Expr Right { get; }
        public EqualOperation Operation { get; }
        public EqualTailExpr? NextTail { get; }

        public EqualTailExpr(Expr right, EqualOperation operation, EqualTailExpr? nextTail)
        {
            Right = right;
            Operation = operation;
            NextTail = nextTail;
        }

        public NuaValue Eval(NuaContext context, NuaValue? leftValue)
        {
            var rightValue = Right.Eval(context);

            var result = Operation switch
            {
                EqualOperation.Equal => Object.Equals(leftValue, rightValue),
                EqualOperation.NotEqual => !Object.Equals(leftValue, rightValue),
                _ => false
            };

            return new NuaBoolean(result);
        }

        public NuaValue Eval(NuaContext context, Expr expr) => Eval(context, expr.Eval(context));

        public override NuaValue? Eval(NuaContext context) => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out EqualTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.OptEql &&
                tokens[cursor].Kind != TokenKind.OptNeq)
                return false;

            EqualOperation operation = tokens[cursor].Kind switch
            {
                TokenKind.OptEql => EqualOperation.Equal,
                TokenKind.OptNeq => EqualOperation.NotEqual,
                _ => EqualOperation.Equal,
            };

            cursor++;

            if (!Expr.Match(ExprLevel.Compare, tokens, ref cursor, out var right))
                return false;

            Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new EqualTailExpr(right, operation, nextTail);
            return true;
        }
    }
}
