using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class QuotedChainExpr : Syntax
    {
        public QuotedChainExpr(ChainExpr chain)
        {
            Chain = chain;
        }

        public ChainExpr Chain { get; }

        public override NuaValue? Evaluate(NuaContext context) => Chain.Evaluate(context);

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out QuotedChainExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.ParenthesesLeft, ref cursor, out _, out _))
            {
                requireMoreTokens = false;
                message = null;
                return false;
            }

            if (!ChainExpr.Match(tokens, true, ref cursor, out requireMoreTokens, out message, out var chain))
                return false;

            if (!TokenMatch(tokens, true, TokenKind.ParenthesesRight, ref cursor, out requireMoreTokens, out _))
            {
                message = "Require ')' after 'chain-expression' while parsing 'quoted-chain-expression'";
                return false;
            }

            index = cursor;
            expr = new QuotedChainExpr(chain);
            return true;
        }
    }
}
