using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AddTailExpr : Expr
    {
        public AddTailExpr(Expr right, AddOperation operation, AddTailExpr? nextTail)
        {
            Right = right;
            Operation = operation;
            NextTail = nextTail;
        }

        public Expr Right { get; }
        public AddOperation Operation { get; }
        public AddTailExpr? NextTail { get; }

        public NuaValue? Evaluate(NuaContext context, string leftValue)
        {
            var rightValue = Right.Evaluate(context);

            if (rightValue == null)
                throw new NuaEvalException("Unable to plus on a null value");
            if (rightValue is not NuaString rightString)
                throw new NuaEvalException("Unable to plus on a non-number value");

            var result = Operation switch
            {
                AddOperation.Add => leftValue + rightString.Value,
                AddOperation.Min => leftValue.Replace(rightString.Value, null),
                _ => leftValue + rightString.Value
            };

            if (NextTail != null)
                return NextTail.Evaluate(context, result);

            return new NuaString(result);
        }

        public NuaValue? Evaluate(NuaContext context, double leftValue)
        {
            var rightValue = Right.Evaluate(context);

            if (rightValue == null)
                throw new NuaEvalException("Unable to plus on a null value");
            if (rightValue is not NuaNumber rightNumber)
                throw new NuaEvalException("Unable to plus on a non-number value");

            var result = Operation switch
            {
                AddOperation.Add => leftValue + rightNumber.Value,
                AddOperation.Min => leftValue - rightNumber.Value,
                _ => leftValue * rightNumber.Value
            };

            if (NextTail != null)
                return NextTail.Evaluate(context, result);

            return new NuaNumber(result);
        }

        public NuaValue? Evaluate(NuaContext context, Expr left)
        {
            var leftValue = left.Evaluate(context);

            if (leftValue == null)
                throw new NuaEvalException("Unable to plus on a null value");
            if (leftValue is NuaNumber leftNumber)
                return Evaluate(context, leftNumber.Value);
            if (leftValue is NuaString leftString)
                return Evaluate(context, leftString.Value);

            throw new NuaEvalException("Plus calculation can be only used on Number and String value");
        }

        public override NuaValue? Evaluate(NuaContext context) => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out AddTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, required, TokenKind.OptAdd, ref cursor, out requireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptMin, ref cursor, out requireMoreTokens, out operatorToken))
            {
                message = null;
                return false;
            }

            var operation = operatorToken.Kind switch
            {
                TokenKind.OptAdd => AddOperation.Add,
                TokenKind.OptMin => AddOperation.Min,
                _ => AddOperation.Add
            };

            if (!MulExpr.Match(tokens, true, ref cursor, out _, out message, out var right))
            {
                requireMoreTokens = true;
                return false;
            }

            if (!Match(tokens, false, ref cursor, out bool tailRequireMoreTokens, out var tailMessage, out var nextTail) && tailRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = tailMessage;
                return false;
            }

            index = cursor;
            expr = new AddTailExpr(right, operation, nextTail);
            requireMoreTokens = false;
            message = null;
            return true;
        }
    }
}
