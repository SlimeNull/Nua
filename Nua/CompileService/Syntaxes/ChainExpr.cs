using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ChainExpr : Syntax
    {
        public ChainExpr(IEnumerable<Expr> expressions)
        {
            Expressions = expressions
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<Expr> Expressions { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            NuaValue? value = null;
            foreach (var expr in Expressions)
                value = expr?.Evaluate(context);

            return value;
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ChainExpr? expr)
        {
            expr = null;
            int cursor = index;

            List<Expr> expressions = new();

            if (!Expr.MatchAny(tokens, ref cursor, out var firstExpr))
                return false;

            expressions.Add(firstExpr);

            while (TokenMatch(tokens, ref cursor, TokenKind.OptComma, out _))
            {
                if (!Expr.MatchAny(tokens, ref cursor, out var nextExpr))
                    throw new NuaParseException("Expect expression after ',' while parsing 'chain-expression'");

                expressions.Add(nextExpr);
            }

            index = cursor;
            expr = new ChainExpr(expressions);
            return true;
        }
    }
}
