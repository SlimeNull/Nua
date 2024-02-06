using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AssignTailExpr : Expr
    {
        public Expr RightExpr { get; }
        public AssignOperation Operation { get; }
        public AssignTailExpr? NextTailExpr { get; }

        public AssignTailExpr(Expr rightExpr, AssignOperation operation, AssignTailExpr? nextTailExpr)
        {
            RightExpr = rightExpr;
            Operation = operation;
            NextTailExpr = nextTailExpr;
        }

        public NuaValue? Evaluate(NuaContext context, Expr leftExpr)
        {
            NuaValue? toAssign = null;
            if (NextTailExpr == null)
                toAssign = RightExpr.Evaluate(context);
            else
                toAssign = NextTailExpr.Evaluate(context, RightExpr);

            var newValue = Operation switch
            {
                AssignOperation.AddWith =>  EvalUtilities.EvalPlus(leftExpr.Evaluate(context), toAssign),
                AssignOperation.MinWith =>  EvalUtilities.EvalMinus(leftExpr.Evaluate(context), toAssign),
                AssignOperation.Assign => toAssign,
                _ => toAssign,
            };

            if (leftExpr is ValueAccessExpr valueAccessExpr)
            {
                valueAccessExpr.SetMemberValue(context, newValue);
                return newValue;
            }
            else if (leftExpr is VariableExpr variableExpr)
            {
                variableExpr.SetValue(context, newValue);
                return newValue;
            }
            else
            {
                throw new NuaEvalException("Only Value member or Variable can be assigned");
            }
        }
        public CompiledSyntax Compile(Expr leftExpr)
        {
            CompiledSyntax compiledLeft = leftExpr.Compile();
            CompiledSyntax compiledToAssign;
            if (NextTailExpr == null)
                compiledToAssign = RightExpr.Compile();
            else
                compiledToAssign = NextTailExpr.Compile(RightExpr);

            CompiledSyntax compiledNewValue = Operation switch
            {
                AssignOperation.AddWith => CompiledSyntax.CreateFromDelegate((context) => EvalUtilities.EvalPlus(compiledLeft.Evaluate(context), compiledToAssign.Evaluate(context))),
                AssignOperation.MinWith => CompiledSyntax.CreateFromDelegate((context) => EvalUtilities.EvalMinus(compiledLeft.Evaluate(context), compiledToAssign.Evaluate(context))),
                AssignOperation.Assign or _ => CompiledSyntax.CreateFromDelegate((context) => compiledToAssign.Evaluate(context)),
            };


            if (leftExpr is ValueAccessExpr valueAccessExpr)
            {
                return CompiledSyntax.CreateFromDelegate((context) =>
                {
                    var newValue = compiledNewValue.Evaluate(context);
                    valueAccessExpr.SetMemberValue(context, compiledNewValue.Evaluate(context));

                    return newValue;
                });
            }
            else if (leftExpr is VariableExpr variableExpr)
            {
                return CompiledSyntax.CreateFromDelegate((context) =>
                {
                    var newValue = compiledNewValue.Evaluate(context);
                    variableExpr.SetValue(context, newValue);

                    return newValue;
                });
            }
            else
            {
                throw new NuaCompileException("Only Value member or Variable can be assigned");
            }
        }

        public override NuaValue? Evaluate(NuaContext context) => throw new InvalidOperationException();
        public override CompiledSyntax Compile() => throw new InvalidOperationException();

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
