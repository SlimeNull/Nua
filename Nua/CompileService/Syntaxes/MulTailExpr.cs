using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class MulTailExpr : Expr
    {
        public MulTailExpr(Expr rightExpr, MulOperation operation, MulTailExpr? nextTailExpr)
        {
            RightExpr = rightExpr;
            Operation = operation;
            NextTailExpr = nextTailExpr;
        }

        public Expr RightExpr { get; }
        public MulOperation Operation { get; }
        public MulTailExpr? NextTailExpr { get; }

        public NuaValue? Evaluate(NuaContext context, double leftValue)
        {
            var rightValue = RightExpr.Evaluate(context);

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

            if (NextTailExpr != null)
                return NextTailExpr.Evaluate(context, result);

            return new NuaNumber(result);
        }

        public NuaValue? Evaluate(NuaContext context, NuaValue? left)
        {
            var rightValue = RightExpr.Evaluate(context);

            NuaValue? result = Operation switch
            {
                MulOperation.Mul => EvalUtilities.EvalMultiply(left, rightValue),
                MulOperation.Div => EvalUtilities.EvalDivide(left, rightValue),
                MulOperation.Pow => EvalUtilities.EvalPower(left, rightValue),
                MulOperation.Mod => EvalUtilities.EvalMod(left, rightValue),
                MulOperation.DivInt => EvalUtilities.EvalDivideInt(left, rightValue),
                _ => EvalUtilities.EvalMultiply(left, rightValue),
            };

            if (NextTailExpr is not null)
                result = NextTailExpr.Evaluate(context, result);

            return result;
        }

        public NuaValue? Evaluate(NuaContext context, Expr left)
        {
            return Evaluate(context, left.Evaluate(context));
        }

        public override NuaValue? Evaluate(NuaContext context) => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out MulTailExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, required, TokenKind.OptMul, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptDiv, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptPow, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptMod, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptDivInt, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken))
            {
                parseStatus.Message = null;
                return false;
            }

            var operation = operatorToken.Kind switch
            {
                TokenKind.OptMul => MulOperation.Mul,
                TokenKind.OptDiv => MulOperation.Div,
                TokenKind.OptPow => MulOperation.Pow,
                TokenKind.OptMod => MulOperation.Mod,
                TokenKind.OptDivInt => MulOperation.DivInt,
                _ => MulOperation.Mul
            };

            if (!ProcessExpr.Match(tokens, true, ref cursor, out parseStatus, out var right))
            {
                parseStatus.Intercept = true;
                if (parseStatus.Message == null)
                    parseStatus.Message = "Expect expression after '*','/','**','//','%' while parsing 'mul-expression'";

                return false;
            }

            if (!Match(tokens, false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = new MulTailExpr(right, operation, nextTail);
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return true;
        }
    }
}
