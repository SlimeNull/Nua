using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class WhileExpr : ProcessExpr
    {
        public Expr ConditionExpr { get; }
        public MultiExpr? BodyExpr { get; }

        public WhileExpr(Expr conditionExpr, MultiExpr? bodyExpr)
        {
            ConditionExpr = conditionExpr;
            BodyExpr = bodyExpr;
        }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            state = EvalState.None;
            NuaValue? result = null;
            while (NuaUtilities.ConditionTest(ConditionExpr.Evaluate(context)))
            {
                EvalState bodyState = EvalState.None;
                result = BodyExpr?.Evaluate(context, out bodyState);

                if (bodyState == EvalState.Continue)
                {
                    continue;
                }
                else if (bodyState == EvalState.Break)
                {
                    break;
                }
                else if (bodyState == EvalState.Return)
                {
                    state = EvalState.Return;
                    break;
                }
            }

            return result;
        }

        public override CompiledProcessSyntax Compile()
        {
            var compiledCondition = ConditionExpr.Compile();
            var compiledBody = BodyExpr?.Compile();

            return CompiledProcessSyntax.CreateFromDelegate(
                delegate (NuaContext context, out EvalState state)
                {
                    NuaValue? result = null;
                    state = EvalState.None;
                    while (EvalUtilities.ConditionTest(compiledCondition.Evaluate(context)))
                    {
                        EvalState bodyState = EvalState.None;
                        result = BodyExpr?.Evaluate(context, out bodyState);

                        if (bodyState == EvalState.Continue)
                        {
                            continue;
                        }
                        else if (bodyState == EvalState.Break)
                        {
                            break;
                        }
                        else if (bodyState == EvalState.Return)
                        {
                            state = EvalState.Return;
                            break;
                        }
                    }

                    return result;
                });
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdWhile, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = required;
                return false;
            }

            if (!Expr.Match(tokens, true, ref cursor, out parseStatus, out var conditionExpr))
            {
                parseStatus.Intercept = true;

                if (parseStatus.Message == null)
                    parseStatus.Message = "Require 'condition-expression' after 'while' keyword while parsing 'while-expression'";

                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require '{' after 'condition-expression' while parsing 'while-expression'";
                return false;
            }

            if (!MultiExpr.Match(tokens, false, ref cursor, out var bodyParseStatus, out var bodyExpr) && bodyParseStatus.Intercept)
            {
                parseStatus = bodyParseStatus;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;

                if (bodyExpr != null)
                    parseStatus.Message = "Require '}' after 'body-expression' while parsing 'while-expression'";
                else
                    parseStatus.Message = "Require 'body-expression' after '{' while parsing 'while-expression'";

                return false;
            }

            index = cursor;
            expr = new WhileExpr(conditionExpr, bodyExpr);
            return true;
        }

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;
            foreach (var syntax in ConditionExpr.TreeEnumerate())
                yield return syntax;

            if (BodyExpr is not null)
                foreach (var syntax in BodyExpr.TreeEnumerate())
                    yield return syntax;
        }
    }
}
