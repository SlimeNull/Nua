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
            if (index < 0 || index >= tokens.Count)
                return false;

            Expr key;

            int cursor = index;
            if (tokens[index].Kind == TokenKind.Identifier)
            {
                key = new ConstExpr(new NuaString(tokens[cursor].Value!));
                cursor++;
            }
            else if (ConstExpr.Match(tokens, ref cursor, out var constKey))
            {
                key = constKey;
                // dont neet to change cursor
            }
            else
            {
                return false;
            }

            if (cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.OptColon)
                return false;
            cursor++;

            if (!Expr.Match(ExprLevel.All, tokens, ref cursor, out var value))
                return false;

            index = cursor;
            expr = new DictMemberExpr(key, value);
            return true;
        }
    }
}
