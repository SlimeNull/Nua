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

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out TableMemberExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            Expr key;
            if (TokenMatch(tokens, required, TokenKind.Identifier, ref cursor, out parseStatus.RequireMoreTokens, out var idToken))
                key = new ConstExpr(new NuaString(idToken.Value!));
            else if (ConstExpr.Match(tokens, required, ref cursor, out parseStatus, out var constKey))
                key = constKey;
            else
                return false;

            if (!TokenMatch(tokens, true, TokenKind.OptColon, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Expect ':' after table member name while parsing 'table-expression'";
                return false;
            }

            if (!Expr.MatchAny(tokens, true, ref cursor, out parseStatus, out var value))
            {
                if (parseStatus.Message == null)
                    parseStatus.Message = "Expect expression after ':' while parsing 'table-expression'";

                return false;
            }

            index = cursor;
            expr = new TableMemberExpr(key, value);
            return true;
        }
    }
}
