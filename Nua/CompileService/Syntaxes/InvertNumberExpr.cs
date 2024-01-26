using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class InvertNumberExpr : UnaryExpr
    {
        public InvertNumberExpr(Expr expr)
        {
            Expr = expr;
        }

        public Expr Expr { get; }

        public override NuaValue? Eval(NuaContext context)
        {
            var value = Expr.Eval(context);

            if (value == null)
                throw new NuaEvalException("Unable to take negation of null value");
            if (value is not NuaNumber number)
                throw new NuaEvalException("Unable to take negation of non-number value");

            return new NuaNumber(-number.Value);
        }

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, ref index, TokenKind.OptMin, out _))
                return false;
            if (!PrimaryExpr.Match(tokens, ref cursor, out var toInvert))
                throw new NuaParseException("Expect 'primary-expression' after '-' while parsing 'invert-number-expression'");

            index = cursor;
            expr = new InvertNumberExpr(toInvert);
            return true;
        }
    }
}
