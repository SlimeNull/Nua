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

        public NuaValue? Eval(NuaContext context, double leftValue)
        {
            var rightValue = Right.Eval(context);

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
                return NextTail.Eval(context, result);

            return new NuaNumber(result);
        }

        public NuaValue? Eval(NuaContext context, Expr left)
        {
            var leftValue = left.Eval(context);

            if (leftValue == null)
                throw new NuaEvalException("Unable to plus on a null value");
            if (leftValue is not NuaNumber leftNumber)
                throw new NuaEvalException("Unable to plus on a non-number value");

            return Eval(context, leftNumber.Value);
        }

        public override NuaValue? Eval(NuaContext context) => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out MulTailExpr? expr)
        {
            expr = null;
            if (index < 0 || index >= tokens.Count)
                return false;
            if (tokens[index].Kind != TokenKind.OptMul &&
                tokens[index].Kind != TokenKind.OptDiv &&
                tokens[index].Kind != TokenKind.OptPow &&
                tokens[index].Kind != TokenKind.OptMod &&
                tokens[index].Kind != TokenKind.OptDivInt)
                return false;

            var operation = tokens[index].Kind switch
            {
                TokenKind.OptMul => MulOperation.Mul,
                TokenKind.OptDiv => MulOperation.Div,
                TokenKind.OptPow => MulOperation.Pow,
                TokenKind.OptMod => MulOperation.Mod,
                TokenKind.OptDivInt => MulOperation.DivInt,
                _ => MulOperation.Mul
            };

            int cursor = index;
            cursor++;

            if (!Expr.Match(ExprLevel.Process, tokens, ref cursor, out var right))
                return false;

            Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new MulTailExpr(right, operation, nextTail);
            return true;
        }
    }
}
