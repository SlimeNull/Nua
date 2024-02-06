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

        public NuaValue? Evaluate(NuaContext context, out bool executed, out EvalState state)
        {
            bool condition = NuaUtilities.ConditionTest(ConditionExpr.Evaluate(context));

            if (condition)
            {
                executed = true;

                if (BodyExpr == null)
                {
                    state = EvalState.None;
                    return null;
                }

                return BodyExpr.Evaluate(context, out state);
            }
            else
            {
                if (ElseIfExpressions != null)
                {
                    foreach (var elseif in ElseIfExpressions)
                    {
                        var value = elseif.Evaluate(context, out var elseIfExecuted, out var elseIfState);

                        if (elseIfExecuted)
                        {
                            executed = true;
                            state = elseIfState;
                            return value;
                        }
                    }
                }

                if (ElseExpr != null)
                {
                    executed = true;
                    return ElseExpr.Evaluate(context, out state);
                }

                executed = false;
                state = EvalState.None;
                return null;
            }
        }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            return Evaluate(context, out _, out state);
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
