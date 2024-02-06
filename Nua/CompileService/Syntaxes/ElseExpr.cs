using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ElseExpr : ProcessExpr
    {
        public MultiExpr? BodyExpr { get; }

        public ElseExpr(MultiExpr? bodyExpr)
        {
            BodyExpr = bodyExpr;
        }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            if (BodyExpr == null)
            {
                state = EvalState.None;
                return null;
            }

            return BodyExpr.Evaluate(context, out state);
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ElseExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdElse, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = required;
                parseStatus.Message = null;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require '{' after 'else' keyword while parsing 'else-expression'";
                return false;
            }

            if (!MultiExpr.Match(tokens, false, ref cursor, out parseStatus, out var body) && parseStatus.Intercept)
                return false;

            if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;

                if (body != null)
                    parseStatus.Message = "Require '}' after '{' while parsing 'else-expression'";
                else
                    parseStatus.Message = "Require body expressions after '{' while parsing 'else-expression'";

                return false;
            }

            index = cursor;
            expr = new ElseExpr(body);
            return true;
        }

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;

            if (BodyExpr is not null)
                foreach (var syntax in BodyExpr.TreeEnumerate())
                    yield return syntax;
        }
    }
}
