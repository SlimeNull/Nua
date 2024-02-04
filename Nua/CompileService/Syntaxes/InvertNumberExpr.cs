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

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.OptMin, ref cursor, out _, out _))
            {
                parseStatus.RequireMoreTokens = false;
                parseStatus.Message = null;
                return false;
            }

            if (!PrimaryExpr.Match(tokens, true, ref cursor, out parseStatus, out var toInvert))
            {
                if (parseStatus.Message == null)
                    parseStatus.Message = "Expect 'primary-expression' after '-' while parsing 'invert-number-expression'";

                return false;
            }

            index = cursor;
            expr = new InvertNumberExpr(toInvert);
            return true;
        }
    }
}
