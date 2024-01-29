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

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ElseExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, ref cursor, TokenKind.KwdElse, out _))
                return false;

            if (!TokenMatch(tokens, ref cursor, TokenKind.BigBracketLeft, out _))
                throw new NuaParseException("Require '{' after 'else' keyword while parsing 'else-expression'");

            MultiExpr.Match(tokens, ref cursor, out var body);

            if (!TokenMatch(tokens, ref cursor, TokenKind.BigBracketRight, out _))
            {
                if (body != null)
                    throw new NuaParseException("Require '}' after '{' while parsing 'else-expression'");
                else
                    throw new NuaParseException("Require body expressions after '{' while parsing 'else-expression'");
            }

            index = cursor;
            expr = new ElseExpr(body);
            return true;
        }
    }
}
