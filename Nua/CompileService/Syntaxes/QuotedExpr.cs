using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class QuotedExpr : ValueExpr
    {
        public QuotedExpr(Expr valueExpr)
        {
            ValueExpr = valueExpr;
        }

        public Expr ValueExpr { get; }

        public override NuaValue? Evaluate(NuaContext context) => ValueExpr.Evaluate(context);

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.ParenthesesLeft, ref cursor, out _, out _))
            {
                parseStatus.RequireMoreTokens = false;
                parseStatus.Message = null;
                return false;
            }

            if (!Expr.Match(tokens, required, ref cursor, out parseStatus, out var content))
            {
                if (parseStatus.Message == null)
                    parseStatus.Message = ("Expect expression after '(' token while parsing 'quote-expressoin'");

                return false;
            }

            if (!TokenMatch(tokens, required, TokenKind.ParenthesesRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Message = "Expect ')' after expression while parsing 'quote-expressoin'";
                return false;
            }

            index = cursor;
            expr = new QuotedExpr(content);
            return true;
        }
    }
}
