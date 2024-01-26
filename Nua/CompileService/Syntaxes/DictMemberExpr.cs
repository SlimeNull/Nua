using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    /// <summary>
    /// xxx: expr,
    /// "xxx": expr
    /// </summary>
    public class DictMemberExpr : Expr
    {
        public DictMemberExpr(Expr key, Expr value)
        {
            Key = key;
            Value = value;
        }

        public Expr Key { get; }
        public Expr Value { get; }

        public override NuaValue? Eval(NuaContext context) => Value.Eval(context);

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out DictMemberExpr? expr)
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
                throw new NuaParseException("Expect ':' after dict member name while parsing 'dict-expression'");

            if (!Expr.MatchAny(tokens, ref cursor, out var value))
                throw new NuaParseException("Expect expression after ':' while parsing 'dict-expression'");

            index = cursor;
            expr = new DictMemberExpr(key, value);
            return true;
        }
    }
}
