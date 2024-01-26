using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    /// <summary>
    /// xxx: expr,
    /// "xxx": expr
    /// </summary>
    public class TableMemberExpr : Expr
    {
        public TableMemberExpr(Expr key, Expr value)
        {
            Key = key;
            Value = value;
        }

        public Expr Key { get; }
        public Expr Value { get; }

        public override NuaValue? Eval(NuaContext context) => Value.Eval(context);

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out TableMemberExpr? expr)
        {
            expr = null;
            int cursor = index;

            Expr key;
            if (TokenMatch(tokens, ref cursor, TokenKind.Identifier, out var idToken))
                key = new ConstExpr(new NuaString(idToken.Value!));
            else if (ConstExpr.Match(tokens, ref cursor, out var constKey))
                key = constKey;
            else
                return false;

            if (!TokenMatch(tokens, ref cursor, TokenKind.OptColon, out _))
                throw new NuaParseException("Expect ':' after table member name while parsing 'table-expression'");

            if (!Expr.MatchAny(tokens, ref cursor, out var value))
                throw new NuaParseException("Expect expression after ':' while parsing 'table-expression'");

            index = cursor;
            expr = new TableMemberExpr(key, value);
            return true;
        }
    }
}
