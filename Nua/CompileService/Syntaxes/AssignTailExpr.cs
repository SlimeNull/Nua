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

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out AssignTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, ref cursor, TokenKind.OptAssign, out operatorToken) &&
                !TokenMatch(tokens, ref cursor, TokenKind.OptAddWith, out operatorToken) &&
                !TokenMatch(tokens, ref cursor, TokenKind.OptMinWith, out operatorToken))
                return false;

            AssignOperation operation = operatorToken.Kind switch
            {
                TokenKind.OptAssign => AssignOperation.Assign,
                TokenKind.OptAddWith => AssignOperation.AddWith,
                TokenKind.OptMinWith => AssignOperation.MinWith,
                _ => default
            };

            if (!OrExpr.Match(tokens, ref cursor, out var right))
                return false;

            Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new AssignTailExpr(right, operation, nextTail);
            return true;
        }
    }
}
