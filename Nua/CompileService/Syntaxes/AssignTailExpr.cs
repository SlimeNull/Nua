using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AssignTailExpr : Expr
    {
        public AssignTailExpr(Expr right, AssignOperation operation, AssignTailExpr? nextTail)
        {
            Right = right;
            Operation = operation;
            NextTail = nextTail;
        }

        public Expr Right { get; }
        public AssignOperation Operation { get; }
        public AssignTailExpr? NextTail { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            if (NextTail == null)
                return Right.Evaluate(context);

            if (Right is ValueAccessExpr valueAccessExpr)
            {
                var value = NextTail.Evaluate(context);
                valueAccessExpr.SetMemberValue(context, value);

                return value;
            }
            else if (Right is VariableExpr variableExpr)
            {
                var value = NextTail.Evaluate(context);
                variableExpr.SetValue(context, value);

                return value;
            }
            else
            {
                throw new NuaEvalException("Only Value member or Variable can be assigned");
            }
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out AssignTailExpr? expr)
        {
            parseStatus = new();
expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, required, TokenKind.OptAssign, ref cursor, out _, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptAddWith, ref cursor, out _, out operatorToken) &&
                !TokenMatch(tokens, required, TokenKind.OptMinWith, ref cursor, out _, out operatorToken))
            {
                parseStatus.RequireMoreTokens = required;
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
