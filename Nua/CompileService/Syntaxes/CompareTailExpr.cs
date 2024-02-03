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

        public NuaValue? Evaluate(NuaContext context, NuaValue? leftValue)
        {
            if (leftValue is not NuaNumber leftNumber)
                throw new NuaEvalException("Unable to compare on a non-number value");
            if (Right.Evaluate(context) is not NuaNumber rightNumber)
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

        public NuaValue? Evaluate(NuaContext context, Expr left)
        {
            return Evaluate(context, left.Evaluate(context));
        }

        public override NuaValue? Evaluate(NuaContext context) => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out CompareTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, required, TokenKind.OptLss, ref cursor, out requireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptGtr, ref cursor, out requireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptLeq, ref cursor, out requireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptGeq, ref cursor, out requireMoreTokens, out operatorToken))
            {
                message = null;
                return false;
            }

            CompareOperation operation = operatorToken.Kind switch
            {
                TokenKind.OptLss => CompareOperation.LessThan,
                TokenKind.OptGtr => CompareOperation.GreaterThan,
                TokenKind.OptLeq => CompareOperation.LessEqual,
                TokenKind.OptGeq => CompareOperation.GreaterEqual,
                _ => CompareOperation.LessThan,
            };

            if (!AddExpr.Match(tokens, true, ref cursor, out requireMoreTokens, out message, out var right))
                return false;

            if (!Match(tokens, false, ref cursor, out var tailRequireMoreTokens, out var tailMessage, out var nextTail) && tailRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = tailMessage;
                return false;
            }

            index = cursor;
            expr = new CompareTailExpr(right, operation, nextTail);
            requireMoreTokens = false;
            message = null;
            return true;
        }
    }
}
