using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class CompareTailExpr : Expr
    {
        public Expr RightExpr { get; }
        public CompareOperation Operation { get; }
        public CompareTailExpr? NextTailExpr { get; }

        public CompareTailExpr(Expr rightExpr, CompareOperation operation, CompareTailExpr? nextTailExpr)
        {
            RightExpr = rightExpr;
            Operation = operation;
            NextTailExpr = nextTailExpr;
        }

        public NuaValue? Evaluate(NuaContext context, NuaValue? leftValue)
        {
            if (leftValue is not NuaNumber leftNumber)
                throw new NuaEvalException("Unable to compare on a non-number value");
            if (RightExpr.Evaluate(context) is not NuaNumber rightNumber)
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

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out CompareTailExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, required, TokenKind.OptLss, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptGtr, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptLeq, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptGeq, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken))
            {
                parseStatus.Message = null;
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

            if (!AddExpr.Match(tokens, true, ref cursor, out parseStatus, out var right))
                return false;

            if (!Match(tokens, false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = new CompareTailExpr(right, operation, nextTail);
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return true;
        }
    }
}
