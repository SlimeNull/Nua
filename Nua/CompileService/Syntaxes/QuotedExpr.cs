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

        public override NuaValue? Eval(NuaContext context) => Content.Eval(context);

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            if (index < 0 || index >= tokens.Count)
                return false;
            if (tokens[index].Kind != TokenKind.ParenthesesLeft)
                return false;

            int cursor = index;
            cursor++;

            if (!Expr.Match(ExprLevel.All, tokens, ref cursor, out var content))
                return false;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.ParenthesesRight)
                return false;

            cursor++;

            expr = new QuotedExpr(content);
            return true;
        }
    }
}
