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

        public NuaValue Evaluate(NuaContext context, NuaValue? leftValue)
        {
            var rightValue = Right.Evaluate(context);

            var result = Operation switch
            {
                EqualOperation.Equal => Object.Equals(leftValue, rightValue),
                EqualOperation.NotEqual => !Object.Equals(leftValue, rightValue),
                _ => false
            };

            return new NuaBoolean(result);
        }

        public NuaValue Evaluate(NuaContext context, Expr expr) => Evaluate(context, expr.Evaluate(context));

        public override NuaValue? Evaluate(NuaContext context) => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out EqualTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, ref cursor, TokenKind.OptEql, out operatorToken) &&
                !TokenMatch(tokens, ref cursor, TokenKind.OptNeq, out operatorToken))
                return false;

            EqualOperation operation = operatorToken.Kind switch
            {
                TokenKind.OptEql => EqualOperation.Equal,
                TokenKind.OptNeq => EqualOperation.NotEqual,
                _ => EqualOperation.Equal,
            };

            if (!CompareExpr.Match(tokens, ref cursor, out var right))
                return false;

            Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new EqualTailExpr(right, operation, nextTail);
            return true;
        }
    }
}
