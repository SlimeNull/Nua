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

        public override NuaValue? Eval(NuaContext context) => Chain.Eval(context);

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out QuotedChainExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, ref cursor, TokenKind.ParenthesesLeft, out _))
                return false;

            if (!ChainExpr.Match(tokens, ref cursor, out var chain))
                return false;

            if (!TokenMatch(tokens, ref cursor, TokenKind.ParenthesesRight, out _))
                return false;

            index = cursor;
            expr = new QuotedChainExpr(chain);
            return true;
        }   
    }
}
