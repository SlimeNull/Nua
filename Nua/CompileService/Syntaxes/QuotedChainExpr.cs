using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class QuotedChainExpr : Syntax
    {
        public QuotedChainExpr(ChainExpr chainExpr)
        {
            ChainExpr = chainExpr;
        }

        public ChainExpr ChainExpr { get; }

        public override NuaValue? Evaluate(NuaContext context) => ChainExpr.Evaluate(context);
        public override CompiledSyntax Compile() => ChainExpr.Compile();

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out QuotedChainExpr? expr)
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

            if (!ChainExpr.Match(tokens, true, ref cursor, out parseStatus, out var chain))
                return false;

            if (!TokenMatch(tokens, true, TokenKind.ParenthesesRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Message = "Require ')' after 'chain-expression' while parsing 'quoted-chain-expression'";
                return false;
            }

            index = cursor;
            expr = new QuotedChainExpr(chain);
            return true;
        }

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;
            foreach (var syntax in ChainExpr.TreeEnumerate())
                yield return syntax;
        }
    }
}
