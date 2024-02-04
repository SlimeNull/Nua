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

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ElseIfExpr? expr)
        {
            parseStatus = new();
expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdElif, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = required;
                parseStatus.Message = null;
                return false;
            }

            if (!Expr.Match(tokens, true, ref cursor, out parseStatus, out var condition))
            {
                parseStatus.Intercept = true;
                if (parseStatus.Message == null)
                    parseStatus.Message = "Require 'elif' condition";

                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require big left bracket after 'elif' condition";
                return false;
            }

            if (!MultiExpr.Match(tokens, false, ref cursor, out parseStatus, out var body) && parseStatus.Intercept)
                return false;

            if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require big right bracket after 'elif' condition";
                return false;
            }

            index = cursor;
            expr = new ElseIfExpr(condition, body);
            return true;
        }
    }
}
