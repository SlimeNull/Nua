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

        public NuaValue? Eval(NuaContext context, string leftValue)
        {
            var rightValue = Right.Eval(context);

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
                return NextTail.Eval(context, result);

            return new NuaString(result);
        }

        public NuaValue? Eval(NuaContext context, double leftValue)
        {
            var rightValue = Right.Eval(context);

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
                return NextTail.Eval(context, result);

            return new NuaNumber(result);
        }

        public NuaValue? Eval(NuaContext context, Expr left)
        {
            var leftValue = left.Eval(context);

            if (leftValue == null)
                throw new NuaEvalException("Unable to plus on a null value");
            if (leftValue is NuaNumber leftNumber)
                return Eval(context, leftNumber.Value);
            if (leftValue is NuaString leftString)
                return Eval(context, leftString.Value);

            throw new NuaEvalException("Plus calculation can be only used on Number and String value");
        }

        public override NuaValue? Eval(NuaContext context) => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out AddTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, ref cursor, TokenKind.OptAdd, out operatorToken) &&
                !TokenMatch(tokens, ref cursor, TokenKind.OptMin, out operatorToken))
                return false;

            var operation = operatorToken.Kind switch
            {
                TokenKind.OptAdd => AddOperation.Add,
                TokenKind.OptMin => AddOperation.Min,
                _ => AddOperation.Add
            };

            if (!MulExpr.Match(tokens, ref cursor, out var right))
                throw new NuaParseException("Expect 'mul-expression' after 'add' or 'min' token");

            Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new AddTailExpr(right, operation, nextTail);
            return true;
        }
    }
}
