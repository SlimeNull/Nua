using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class MulTailExpr : Expr
    {
        public MulTailExpr(Expr right, MulOperation operation, MulTailExpr? nextTail)
        {
            Right = right;
            Operation = operation;
            NextTail = nextTail;
        }

        public Expr Right { get; }
        public MulOperation Operation { get; }
        public MulTailExpr? NextTail { get; }

        public NuaValue? Evaluate(NuaContext context, double leftValue)
        {
            var rightValue = Right.Evaluate(context);

            if (rightValue == null)
                throw new NuaEvalException("Unable to plus on a null value");
            if (rightValue is not NuaNumber rightNumber)
                throw new NuaEvalException("Unable to plus on a non-number value");

            var result = Operation switch
            {
                MulOperation.Mul => leftValue * rightNumber.Value,
                MulOperation.Div => leftValue / rightNumber.Value,
                MulOperation.Pow => Math.Pow(leftValue, rightNumber.Value),
                MulOperation.Mod => leftValue % rightNumber.Value,
                MulOperation.DivInt => Math.Floor(leftValue / rightNumber.Value),
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
            if (leftValue is not NuaNumber leftNumber)
                throw new NuaEvalException("Unable to plus on a non-number value");

            return Evaluate(context, leftNumber.Value);
        }

        public override NuaValue? Evaluate(NuaContext context) => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out MulTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, ref cursor, TokenKind.OptMul, out operatorToken) &&
                !TokenMatch(tokens, ref cursor, TokenKind.OptDiv, out operatorToken) &&
                !TokenMatch(tokens, ref cursor, TokenKind.OptPow, out operatorToken) &&
                !TokenMatch(tokens, ref cursor, TokenKind.OptMod, out operatorToken) &&
                !TokenMatch(tokens, ref cursor, TokenKind.OptDivInt, out operatorToken))
                return false;

            var operation = operatorToken.Kind switch
            {
                TokenKind.OptMul => MulOperation.Mul,
                TokenKind.OptDiv => MulOperation.Div,
                TokenKind.OptPow => MulOperation.Pow,
                TokenKind.OptMod => MulOperation.Mod,
                TokenKind.OptDivInt => MulOperation.DivInt,
                _ => MulOperation.Mul
            };

            if (!ProcessExpr.Match(tokens, ref cursor, out var right))
                throw new NuaParseException("Expect expression after '*','/','**','//','%' while parsing 'mul-expression'");

            Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new MulTailExpr(right, operation, nextTail);
            return true;
        }
    }
}
