using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class QuotedExpr : ValueExpr
    {
        public QuotedExpr(Expr content)
        {
            Content = content;
        }

        public Expr Content { get; }

        public override NuaValue? Evaluate(NuaContext context) => Content.Evaluate(context);

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.ParenthesesLeft, ref cursor, out _, out _))
            {
                requireMoreTokens = false;
                message = null;
                return false;
            }

            if (!Expr.MatchAny(tokens, required, ref cursor, out requireMoreTokens, out message, out var content))
            {
                if (message == null)
                    message = ("Expect expression after '(' token while parsing 'quote-expressoin'");

                return false;
            }

            if (!TokenMatch(tokens, required, TokenKind.ParenthesesRight, ref cursor, out requireMoreTokens, out _))
            {
                message = "Expect ')' after expression while parsing 'quote-expressoin'";
                return false;
            }

            index = cursor;
            expr = new QuotedExpr(content);
            return true;
        }
    }
}
