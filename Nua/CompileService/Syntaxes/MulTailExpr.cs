using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class MulTailExpr : Expr
    {
        public Expr RightExpr { get; }
        public MulOperation Operation { get; }
        public MulTailExpr? NextTailExpr { get; }

        public MulTailExpr(Expr rightExpr, MulOperation operation, MulTailExpr? nextTailExpr)
        {
            RightExpr = rightExpr;
            Operation = operation;
            NextTailExpr = nextTailExpr;
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

        public CompiledSyntax Compile(CompiledSyntax compiledLeft)
        {
            var compiledRight = RightExpr.Compile();

            CompiledSyntax result = Operation switch
            {
                MulOperation.Div => CompiledSyntax.CreateFromDelegate((context) =>
                {
                    var left = compiledLeft.Evaluate(context);
                    var right = compiledRight.Evaluate(context);

                    return EvalUtilities.EvalDivide(left, right);
                })
                ,
                MulOperation.Pow => CompiledSyntax.CreateFromDelegate((context) =>
                {
                    var left = compiledLeft.Evaluate(context);
                    var right = compiledRight.Evaluate(context);

                    return EvalUtilities.EvalPower(left, right);
                })
                ,
                MulOperation.Mod => CompiledSyntax.CreateFromDelegate((context) =>
                {
                    var left = compiledLeft.Evaluate(context);
                    var right = compiledRight.Evaluate(context);

                    return EvalUtilities.EvalMod(left, right);
                })
                ,
                MulOperation.DivInt => CompiledSyntax.CreateFromDelegate((context) =>
                {
                    var left = compiledLeft.Evaluate(context);
                    var right = compiledRight.Evaluate(context);

                    return EvalUtilities.EvalDivideInt(left, right);
                })
                ,
                MulOperation.Mul or _ => CompiledSyntax.CreateFromDelegate((context) =>
                {
                    var left = compiledLeft.Evaluate(context);
                    var right = compiledRight.Evaluate(context);

                    return EvalUtilities.EvalMultiply(left, right);
                })
            };

            if (NextTailExpr is not null)
                result = NextTailExpr.Compile(result);

            return result;
        }
        public CompiledSyntax Compile(Expr leftExpr)
        {
            var compiledLeft = leftExpr.Compile();
            var compiledRight = RightExpr.Compile();

            CompiledSyntax result = Operation switch
            {
                MulOperation.Div => CompiledSyntax.CreateFromDelegate((context) =>
                {
                    var left = compiledLeft.Evaluate(context);
                    var right = compiledRight.Evaluate(context);

                    return EvalUtilities.EvalDivide(left, right);
                })
                ,
                MulOperation.Pow => CompiledSyntax.CreateFromDelegate((context) =>
                {
                    var left = compiledLeft.Evaluate(context);
                    var right = compiledRight.Evaluate(context);

                    return EvalUtilities.EvalPower(left, right);
                })
                ,
                MulOperation.Mod => CompiledSyntax.CreateFromDelegate((context) =>
                {
                    var left = compiledLeft.Evaluate(context);
                    var right = compiledRight.Evaluate(context);

                    return EvalUtilities.EvalMod(left, right);
                })
                ,
                MulOperation.DivInt => CompiledSyntax.CreateFromDelegate((context) =>
                {
                    var left = compiledLeft.Evaluate(context);
                    var right = compiledRight.Evaluate(context);

                    return EvalUtilities.EvalDivideInt(left, right);
                })
                ,
                MulOperation.Mul or _ => CompiledSyntax.CreateFromDelegate((context) =>
                {
                    var left = compiledLeft.Evaluate(context);
                    var right = compiledRight.Evaluate(context);

                    return EvalUtilities.EvalMultiply(left, right);
                })
            };

            if (NextTailExpr is not null)
                result = NextTailExpr.Compile(result);

            return result;
        }

        public override NuaValue? Evaluate(NuaContext context) => throw new InvalidOperationException();
        public override CompiledSyntax Compile() => throw new InvalidOperationException();

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
