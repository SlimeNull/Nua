using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ElseExpr : ProcessExpr
    {
        public ElseExpr(MultiExpr? body)
        {
            Body = body;
        }

        public MultiExpr? Body { get; }

        public override NuaValue? Eval(NuaContext context, out EvalState state)
        {
            if (Body == null)
            {
                state = EvalState.None;
                return null;
            }

            return Body.Eval(context, out state);
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ElseExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.KwdElse)
                return false;
            cursor++;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.BigBracketLeft)
                return false;
            cursor++;

            MultiExpr.Match(tokens, ref cursor, out var body);

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.BigBracketRight)
                return false;
            cursor++;

            index = cursor;
            expr = new ElseExpr(body);
            return true;
        }
    }
}
