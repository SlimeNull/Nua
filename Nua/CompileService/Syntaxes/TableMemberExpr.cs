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
        public TableMemberExpr(Expr keyExpr, Expr valueExpr)
        {
            KeyExpr = keyExpr;
            ValueExpr = valueExpr;
        }

        public Expr KeyExpr { get; }
        public Expr ValueExpr { get; }

        public override NuaValue? Evaluate(NuaContext context) => ValueExpr.Evaluate(context);

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

            if (!Expr.Match(tokens, true, ref cursor, out parseStatus, out var value))
            {
                if (parseStatus.Message == null)
                    parseStatus.Message = "Expect expression after ':' while parsing 'table-expression'";

                return false;
            }

            index = cursor;
            expr = new TableMemberExpr(key, value);
            return true;
        }

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;
            foreach (var syntax in KeyExpr.TreeEnumerate())
                yield return syntax;
            foreach (var syntax in ValueExpr.TreeEnumerate())
                yield return syntax;
        }
    }
}
