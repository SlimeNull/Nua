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

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, ref cursor, TokenKind.ParenthesesLeft, out _))
                return false;

            if (!Expr.MatchAny(tokens, ref cursor, out var content))
                throw new NuaParseException("Expect expression after '(' token while parsing 'quote-expressoin'");

            if (!TokenMatch(tokens, ref cursor, TokenKind.ParenthesesRight, out _))
                throw new NuaParseException("Expect ')' after expression while parsing 'quote-expressoin'");

            index = cursor;
            expr = new QuotedExpr(content);
            return true;
        }
    }
}
