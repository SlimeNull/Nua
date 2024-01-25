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

        public NuaValue? Eval(NuaContext context, out bool executed, out EvalState state)
        {
            bool condition = IfExpr.Test(Condition.Eval(context));

            if (condition)
            {
                executed = true;

                if (Body == null)
                {
                    state = EvalState.None;
                    return null;
                }

                return Body.Eval(context, out state);
            }

            executed = false;
            state = EvalState.None;
            return null;
        }

        public override NuaValue? Eval(NuaContext context, out EvalState state)
        {
            return Eval(context, out _, out state);
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ElseIfExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.KwdElif)
                return false;
            cursor++;

            if (!Expr.Match(ExprLevel.All, tokens, ref cursor, out var condition))
                throw new NuaParseException("Require 'elif' condition");

            if (cursor < 0 || cursor >= tokens.Count ||
                tokens[cursor].Kind != TokenKind.BigBracketLeft)
                throw new NuaParseException("Require big left bracket after 'elif' condition");
            cursor++;

            MultiExpr.Match(tokens, ref cursor, out var body);

            if (cursor < 0 || cursor >= tokens.Count ||
                tokens[cursor].Kind != TokenKind.BigBracketRight)
                throw new NuaParseException("Require big right bracket after 'elif' body expressions");
            cursor++;

            index = cursor;
            expr = new ElseIfExpr(condition, body);
            return true;
        }
    }
}
