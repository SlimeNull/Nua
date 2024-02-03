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

            NuaContext ifContext = new NuaContext(context);

            if (condition)
            {
                executed = true;

                if (Body == null)
                {
                    state = EvalState.None;
                    return null;
                }

                return Body.Evaluate(ifContext, out state);
            }
            else
            {
                if (ElseIfExpressions != null)
                {
                    foreach (var elseif in ElseIfExpressions)
                    {
                        var value = elseif.Evaluate(ifContext, out var elseIfExecuted, out var elseIfState);

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
                    return ElseExpressions.Evaluate(ifContext, out state);
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

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdIf, ref cursor, out _, out _))
            {
                requireMoreTokens = false;
                message = null;
                return false;
            }

            if (!Expr.MatchAny(tokens, true, ref cursor, out requireMoreTokens, out message, out var condition))
            {
                if (message == null)
                    message = "Require 'if' condition";

                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out requireMoreTokens, out _))
            {
                message = "Require big left bracket after 'if' condition";
                return false;
            }

            if (!MultiExpr.Match(tokens, false, ref cursor, out var bodyRequireMoreTokens, out var bodyMessage, out var body) && bodyRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = bodyMessage;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out requireMoreTokens, out _))
            {
                message = "Require bit right bracket after 'if body' expressions";
                return false;
            }

            List<ElseIfExpr>? elseifs = null;

            while (ElseIfExpr.Match(tokens, false, ref cursor, out requireMoreTokens, out message, out var elseif))
            {
                if (elseifs == null)
                    elseifs = new();

                elseifs.Add(elseif);
            }

            if (requireMoreTokens)
                return false;

            if (!ElseExpr.Match(tokens, false, ref cursor, out var elseRequireMoreTokens, out var elseMessage, out var elseExpr) && elseRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = elseMessage;
                return false;
            }

            index = cursor;
            expr = new IfExpr(condition, body, elseifs, elseExpr);
            return true;
        }
    }
}
