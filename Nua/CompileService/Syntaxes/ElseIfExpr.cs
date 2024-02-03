using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ElseIfExpr : ProcessExpr
    {
        public ElseIfExpr(Expr condition, MultiExpr? body)
        {
            Condition = condition;
            Body = body;
        }

        public Expr Condition { get; }
        public MultiExpr? Body { get; }

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

            executed = false;
            state = EvalState.None;
            return null;
        }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            return Evaluate(context, out _, out state);
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out ElseIfExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdElif, ref cursor, out requireMoreTokens, out _))
            {
                message = null;
                return false;
            }

            if (!Expr.MatchAny(tokens, true, ref cursor, out requireMoreTokens, out message, out var condition))
            {
                if (message == null)
                    message = "Require 'elif' condition";

                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out requireMoreTokens, out _))
            {
                message = "Require big left bracket after 'elif' condition";
                return false;
            }

            if (!MultiExpr.Match(tokens, false, ref cursor, out requireMoreTokens, out message, out var body) && requireMoreTokens)
                return false;

            if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out requireMoreTokens, out _))
            {
                message = "Require big right bracket after 'elif' condition";
                return false;
            }

            index = cursor;
            expr = new ElseIfExpr(condition, body);
            return true;
        }
    }
}
