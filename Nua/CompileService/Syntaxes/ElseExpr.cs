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

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            if (Body == null)
            {
                state = EvalState.None;
                return null;
            }

            return Body.Evaluate(context, out state);
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out ElseExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdElse, ref cursor, out _, out _))
            {
                requireMoreTokens = false;
                message = null;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out requireMoreTokens, out _))
            {
                message = "Require '{' after 'else' keyword while parsing 'else-expression'";
                return false;
            }

            if (!MultiExpr.Match(tokens, false, ref cursor, out requireMoreTokens, out message, out var body) && requireMoreTokens)
                return false;

            if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out requireMoreTokens, out _))
            {
                if (body != null)
                    message = "Require '}' after '{' while parsing 'else-expression'";
                else
                    message = "Require body expressions after '{' while parsing 'else-expression'";

                return false;
            }

            index = cursor;
            expr = new ElseExpr(body);
            return true;
        }
    }
}
