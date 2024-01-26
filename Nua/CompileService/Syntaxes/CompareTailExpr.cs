using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class CompareTailExpr : Expr
    {
        public Expr Right { get; }
        public CompareOperation Operation { get; }
        public CompareTailExpr? NextTail { get; }

        public CompareTailExpr(Expr right, CompareOperation operation, CompareTailExpr? nextTail)
        {
            Right = right;
            Operation = operation;
            NextTail = nextTail;
        }

        public NuaValue? Eval(NuaContext context, NuaValue? leftValue)
        {
            if (leftValue is not NuaNumber leftNumber)
                throw new NuaEvalException("Unable to compare on a non-number value");
            if (Right.Eval(context) is not NuaNumber rightNumber)
                throw new NuaEvalException("Unable to compare on a non-number value");

            bool result = Operation switch
            {
                CompareOperation.LessThan => leftNumber.Value < rightNumber.Value,
                CompareOperation.GreaterThan => leftNumber.Value > rightNumber.Value,
                CompareOperation.LessEqual => leftNumber.Value <= rightNumber.Value,
                CompareOperation.GreaterEqual => leftNumber.Value >= rightNumber.Value,
                _ => leftNumber.Value < rightNumber.Value,
            };

            return new NuaBoolean(result);
        }

        public NuaValue? Eval(NuaContext context, Expr left)
        {
            return Eval(context, left.Eval(context));
        }

        public override NuaValue? Eval(NuaContext context) => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out CompareTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, ref cursor, TokenKind.OptLss, out operatorToken) &&
                !TokenMatch(tokens, ref cursor, TokenKind.OptGtr, out operatorToken) &&
                !TokenMatch(tokens, ref cursor, TokenKind.OptLeq, out operatorToken) &&
                !TokenMatch(tokens, ref cursor, TokenKind.OptGeq, out operatorToken))
                return false;

            CompareOperation operation = operatorToken.Kind switch
            {
                TokenKind.OptLss => CompareOperation.LessThan,
                TokenKind.OptGtr => CompareOperation.GreaterThan,
                TokenKind.OptLeq => CompareOperation.LessEqual,
                TokenKind.OptGeq => CompareOperation.GreaterEqual,
                _ => CompareOperation.LessThan,
            };

            if (!AddExpr.Match(tokens, ref cursor, out var right))
                return false;

            Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new CompareTailExpr(right, operation, nextTail);
            return true;
        }
    }
}
