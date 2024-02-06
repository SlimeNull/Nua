using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class IfExpr : ProcessExpr
    {
        public Expr ConditionExpr { get; }
        public MultiExpr? BodyExpr { get; }
        public IReadOnlyList<ElseIfExpr>? ElseIfExpressions { get; }
        public ElseExpr? ElseExpr { get; }

        public IfExpr(Expr conditionExpr, MultiExpr? bodyExpr, IEnumerable<ElseIfExpr>? elseIfExpressions, ElseExpr? elseExpr)
        {
            ConditionExpr = conditionExpr;
            BodyExpr = bodyExpr;
            ElseIfExpressions = elseIfExpressions?.ToList()?.AsReadOnly();
            ElseExpr = elseExpr;
        }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            state = EvalState.None;

            if (NuaUtilities.ConditionTest(ConditionExpr.Evaluate(context)))
            {
                return BodyExpr?.Evaluate(context, out state);
            }
            else
            {
                if (ElseIfExpressions != null)
                {
                    foreach (var elseif in ElseIfExpressions)
                    {
                        if (EvalUtilities.ConditionTest(elseif.ConditionExpr.Evaluate(context)))
                        {
                            return elseif.BodyExpr?.Evaluate(context, out state);
                        }
                    }
                }

                return ElseExpr?.BodyExpr?.Evaluate(context, out state);
            }
        }

        public override CompiledProcessSyntax Compile()
        {
            CompiledSyntax compiledCondition = ConditionExpr.Compile();
            CompiledProcessSyntax? compiledBody = BodyExpr?.Compile();

            List<(CompiledSyntax Condition, CompiledProcessSyntax? Body)> compiledElseIfSyntaxes = new();
            CompiledProcessSyntax? compiledElseBody = ElseExpr?.BodyExpr?.Compile() as CompiledProcessSyntax;

            if (ElseIfExpressions is not null)
                foreach (var elseif in ElseIfExpressions)
                    compiledElseIfSyntaxes.Add((elseif.ConditionExpr.Compile(), elseif.BodyExpr?.Compile()));

            return CompiledProcessSyntax.CreateFromDelegate(
                delegate (NuaContext context, out EvalState state)
                {
                    state = EvalState.None;
                    if (EvalUtilities.ConditionTest(compiledCondition.Evaluate(context)))
                    {
                        return compiledBody?.Evaluate(context, out state);
                    }
                    else
                    {
                        foreach (var compiledElseIf in compiledElseIfSyntaxes)
                        {
                            if (EvalUtilities.ConditionTest(compiledElseIf.Condition.Evaluate(context)))
                            {
                                return compiledElseIf.Body?.Evaluate(context, out state);
                            }
                        }

                        return compiledElseBody?.Evaluate(context, out state);
                    }
                });
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdIf, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = required;
                parseStatus.Message = null;
                return false;
            }

            if (!Expr.Match(tokens, true, ref cursor, out parseStatus, out var condition))
            {
                parseStatus.Intercept = true;
                if (parseStatus.Message == null)
                    parseStatus.Message = "Require 'if' condition";

                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require '{' after 'if' condition";
                return false;
            }

            if (!MultiExpr.Match(tokens, false, ref cursor, out var bodyParseStatus, out var body) && bodyParseStatus.Intercept)
            {
                parseStatus = bodyParseStatus;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require '}' after 'if body' expressions";
                return false;
            }

            List<ElseIfExpr>? elseifs = null;

            while (ElseIfExpr.Match(tokens, false, ref cursor, out parseStatus, out var elseif))
            {
                if (elseifs == null)
                    elseifs = new();

                elseifs.Add(elseif);
            }

            if (parseStatus.Intercept)
                return false;

            if (!ElseExpr.Match(tokens, false, ref cursor, out var elseParseStatus, out var elseExpr) && elseParseStatus.Intercept)
            {
                parseStatus = elseParseStatus;
                return false;
            }

            index = cursor;
            expr = new IfExpr(condition, body, elseifs, elseExpr);
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

            if (ElseIfExpressions is not null)
                foreach (var expr in ElseIfExpressions)
                    foreach (var syntax in expr.TreeEnumerate())
                        yield return syntax;

            if (ElseExpr is not null)
                foreach (var syntax in ElseExpr.TreeEnumerate())
                    yield return syntax;
        }
    }
}
