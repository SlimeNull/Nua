using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class IfExpr : ProcessExpr
    {
        public IfExpr(Expr condition, MultiExpr? body, IEnumerable<ElseIfExpr>? elseIfExpressions, ElseExpr? elseExpressions)
        {
            Condition = condition;
            Body = body;
            ElseIfExpressions = elseIfExpressions?.ToList()?.AsReadOnly();
            ElseExpressions = elseExpressions;
        }

        public Expr Condition { get; }
        public MultiExpr? Body { get; }
        public IReadOnlyList<ElseIfExpr>? ElseIfExpressions { get; }
        public ElseExpr? ElseExpressions { get; }

        public NuaValue? Evaluate(NuaContext context, out bool executed, out EvalState state)
        {
            bool condition = NuaUtilities.ConditionTest(Condition.Evaluate(context));

            if (condition)
            {
                executed = true;

                if (Body == null)
                {
                    state = EvalState.None;
                    return null;
                }

                return Body.Evaluate(context, out state);
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

                if (ElseExpressions != null)
                {
                    executed = true;
                    return ElseExpressions.Evaluate(context, out state);
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

            if (!Expr.MatchAny(tokens, true, ref cursor, out parseStatus, out var condition))
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
    }
}
