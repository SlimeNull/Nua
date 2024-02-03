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

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out ChainExpr? expr)
        {
            expr = null;
            int cursor = index;

            List<Expr> expressions = new();

            if (!Expr.MatchAny(tokens, required, ref cursor, out requireMoreTokens, out message, out var firstExpr))
                return false;

            expressions.Add(firstExpr);

            while (TokenMatch(tokens, false, TokenKind.OptComma, ref cursor, out _, out _))
            {
                if (!Expr.MatchAny(tokens, true, ref cursor, out requireMoreTokens, out message, out var nextExpr))
                {
                    if (message == null)
                        message = "Expect expression after ',' while parsing 'chain-expression'";

                    return false;
                }

                expressions.Add(nextExpr);
            }

            index = cursor;
            expr = new ChainExpr(expressions);
            return true;
        }
    }
}
