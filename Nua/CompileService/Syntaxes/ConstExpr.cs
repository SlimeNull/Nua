using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ConstExpr : ValueExpr
    {
        public ConstExpr(NuaValue? value)
        {
            Value = value;
        }

        public NuaValue? Value { get; }

        public override NuaValue? Evaluate(NuaContext context) => Value;

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
expr = null;
            if (index < 0 || index >= tokens.Count)
            {
                parseStatus.RequireMoreTokens = required;
                parseStatus.Message = null;
                return false;
            }

            var token = tokens[index];

            if (token.Kind == TokenKind.Number && token.Value != null)
                expr = new ConstExpr(new NuaNumber(double.Parse(token.Value)));
            else if (token.Kind == TokenKind.String && token.Value != null)
                expr = new ConstExpr(new NuaString(token.Value));
            else if (token.Kind == TokenKind.KwdTrue)
                expr = new ConstExpr(new NuaBoolean(true));
            else if (token.Kind == TokenKind.KwdFalse)
                expr = new ConstExpr(new NuaBoolean(false));
            else if (token.Kind == TokenKind.KwdNull)
                expr = new ConstExpr(null);

            if (expr != null)
                index++;

            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return expr != null;
        }
    }
}
