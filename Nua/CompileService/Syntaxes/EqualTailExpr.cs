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

        public NuaValue? Evaluate(NuaContext context, NuaValue? leftValue)
        {
            var rightValue = RightExpr.Evaluate(context);

            NuaValue? result = Operation switch
            {
                EqualOperation.Equal => EvalUtilities.EvalEqual(leftValue, rightValue),
                EqualOperation.NotEqual => EvalUtilities.EvalNotEqual(leftValue, rightValue),
                _ => null
            };

            if (NextTailExpr is not null)
                result = NextTailExpr.Evaluate(context, result);

            return result;
        }

        public NuaValue? Evaluate(NuaContext context, Expr leftExpr) => Evaluate(context, leftExpr.Evaluate(context));

        public CompiledSyntax Compile(CompiledSyntax compiledLeft)
        {
            var compiledRight = RightExpr.Compile();

            CompiledSyntax result = Operation switch
            {
                EqualOperation.Equal => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalEqual(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
                EqualOperation.NotEqual => CompiledSyntax.CreateFromDelegate(context => EvalUtilities.EvalNotEqual(compiledLeft.Evaluate(context), compiledRight.Evaluate(context))),
                _ => throw new InvalidOperationException("Invalid equal operation")
            };

            if (NextTailExpr is not null)
                result = NextTailExpr.Compile(result);

            return result;
        }
        public CompiledSyntax Compile(Expr leftExpr) => Compile(leftExpr.Compile());

        public override NuaValue? Evaluate(NuaContext context) => throw new InvalidOperationException();
        public override CompiledSyntax Compile() => throw new InvalidOperationException();

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

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;
            foreach (var syntax in RightExpr.TreeEnumerate())
                yield return syntax;

            if (NextTailExpr is not null)
                foreach (var syntax in NextTailExpr.TreeEnumerate())
                    yield return syntax;
        }
    }
}
