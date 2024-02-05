using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class EqualTailExpr : Expr
    {
        public Expr RightExpr { get; }
        public EqualOperation Operation { get; }
        public EqualTailExpr? NextTailExpr { get; }

        public EqualTailExpr(Expr rightExpr, EqualOperation operation, EqualTailExpr? nextTailExpr)
        {
            RightExpr = rightExpr;
            Operation = operation;
            NextTailExpr = nextTailExpr;
        }

        public NuaValue Evaluate(NuaContext context, NuaValue? leftValue)
        {
            var rightValue = RightExpr.Evaluate(context);

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

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out EqualTailExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, required, TokenKind.OptEql, ref cursor, out _, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptNeq, ref cursor, out _, out operatorToken))
            {
                parseStatus.RequireMoreTokens = required;
                parseStatus.Message = null;
                return false;
            }

            EqualOperation operation = operatorToken.Kind switch
            {
                TokenKind.OptEql => EqualOperation.Equal,
                TokenKind.OptNeq => EqualOperation.NotEqual,
                _ => EqualOperation.Equal,
            };

            if (!CompareExpr.Match(tokens, true, ref cursor, out parseStatus, out var right))
            {
                parseStatus.Intercept = true;
                return false;
            }

            if (!Match(tokens, false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = new EqualTailExpr(right, operation, nextTail);
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return true;
        }
    }
}
