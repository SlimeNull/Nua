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

        public override NuaValue? Eval(NuaContext context)
        {
            NuaValue? value = null;
            foreach (var expr in Expressions)
                value = expr?.Eval(context);

            return value;
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ChainExpr? expr)
        {
            expr = null;
            int cursor = index;

            List<Expr> expressions = new();

            if (!Expr.Match(ExprLevel.All, tokens, ref cursor, out var firstExpr))
                return false;

            expressions.Add(firstExpr);

            while (cursor >= 0 && cursor < tokens.Count)
            {
                if (tokens[cursor].Kind != TokenKind.OptComma)
                    break;

                cursor++;

                if (!Expr.Match(ExprLevel.All, tokens, ref cursor, out var nextExpr))
                    throw new NuaParseException("Expression is required");

                expressions.Add(nextExpr);
            }

            index = cursor;
            expr = new ChainExpr(expressions);
            return true;
        }
    }
}
