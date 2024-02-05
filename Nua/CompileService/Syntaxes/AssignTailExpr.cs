using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AssignTailExpr : Expr
    {
        public AssignTailExpr(Expr rightExpr, AssignOperation operation, AssignTailExpr? nextTailExpr)
        {
            RightExpr = rightExpr;
            Operation = operation;
            NextTailExpr = nextTailExpr;
        }

        public Expr RightExpr { get; }
        public AssignOperation Operation { get; }
        public AssignTailExpr? NextTailExpr { get; }

        public NuaValue? Evaluate(NuaContext context, Expr left)
        {
            NuaValue? toAssign = null;
            if (NextTailExpr == null)
                toAssign = RightExpr.Evaluate(context);
            else
                toAssign = NextTailExpr.Evaluate(context, RightExpr);

            var newValue = Operation switch
            {
                AssignOperation.AddWith =>  EvalUtilities.EvalPlus(left.Evaluate(context), toAssign),
                AssignOperation.MinWith =>  EvalUtilities.EvalMinus(left.Evaluate(context), toAssign),
                AssignOperation.Assign => toAssign,
                _ => toAssign,
            };

            if (left is ValueAccessExpr valueAccessExpr)
            {
                valueAccessExpr.SetMemberValue(context, newValue);
                return newValue;
            }
            else if (left is VariableExpr variableExpr)
            {
                variableExpr.SetValue(context, newValue);
                return newValue;
            }
            else
            {
                throw new NuaEvalException("Only Value member or Variable can be assigned");
            }
        }

        public override NuaValue? Evaluate(NuaContext context)
        {
            throw new InvalidOperationException();
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out AssignTailExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, required, TokenKind.OptAssign, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptAddWith, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptMinWith, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken))
            {
                parseStatus.Intercept = required;
                parseStatus.Message = null;
                return false;
            }

            AssignOperation operation = operatorToken.Kind switch
            {
                TokenKind.OptAssign => AssignOperation.Assign,
                TokenKind.OptAddWith => AssignOperation.AddWith,
                TokenKind.OptMinWith => AssignOperation.MinWith,
                _ => default
            };

            if (!OrExpr.Match(tokens, required, ref cursor, out parseStatus, out var right))
            {
                if (parseStatus.Message == null)
                    parseStatus.Message = "Require expression after '=' token while parsing 'assign-tail-expression'";

                return false;
            }

            if (!Match(tokens, false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = new AssignTailExpr(right, operation, nextTail);
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return true;
        }
    }
}
