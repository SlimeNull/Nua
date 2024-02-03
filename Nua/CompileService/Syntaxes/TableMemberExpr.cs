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

        public override NuaValue? Evaluate(NuaContext context) => Value.Evaluate(context);

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out TableMemberExpr? expr)
        {
            expr = null;
            int cursor = index;

            Expr key;
            if (TokenMatch(tokens, required, TokenKind.Identifier, ref cursor, out _, out var idToken))
                key = new ConstExpr(new NuaString(idToken.Value!));
            else if (ConstExpr.Match(tokens, required, ref cursor, out requireMoreTokens, out message, out var constKey))
                key = constKey;
            else
                return false;

            if (!TokenMatch(tokens, true, TokenKind.OptColon, ref cursor, out _, out _))
            {
                requireMoreTokens = true;
                message = "Expect ':' after table member name while parsing 'table-expression'";
                return false;
            }

            if (!Expr.MatchAny(tokens, true, ref cursor, out requireMoreTokens, out message, out var value))
            {
                if (message == null)
                    message = "Expect expression after ':' while parsing 'table-expression'";

                return false;
            }

            index = cursor;
            expr = new TableMemberExpr(key, value);
            return true;
        }
    }
}
