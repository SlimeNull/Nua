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

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out InvertNumberExpr? expr)
        {
            expr = null;
            if (index < 0 || index >= tokens.Count)
                return false;
            if (tokens[index].Kind != TokenKind.OptMin)
                return false;

            int cursor = index;
            cursor++;

            if (!Syntaxes.Expr.Match(ExprLevel.Primary, tokens, ref cursor, out var toInvert))
                return false;

            index = cursor;
            expr = new InvertNumberExpr(toInvert);
            return true;
        }
    }
}
