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

        public override NuaValue? Evaluate(NuaContext context)
        {
            var value = Expr.Evaluate(context);

            if (value == null)
                throw new NuaEvalException("Unable to take negation of null value");
            if (value is not NuaNumber number)
                throw new NuaEvalException("Unable to take negation of non-number value");

            return new NuaNumber(-number.Value);
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.OptMin, ref cursor, out _, out _))
            {
                requireMoreTokens = false;
                message = null;
                return false;
            }

            if (!PrimaryExpr.Match(tokens, true, ref cursor, out requireMoreTokens, out message, out var toInvert))
            {
                if (message == null)
                    message = "Expect 'primary-expression' after '-' while parsing 'invert-number-expression'";

                return false;
            }

            index = cursor;
            expr = new InvertNumberExpr(toInvert);
            return true;
        }
    }
}
