using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ReturnExpr : ProcessExpr
    {
        public Expr? Value { get; }

        public ReturnExpr(Expr? value)
        {
            Value = value;
        }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            state = EvalState.Return;
            return Value?.Evaluate(context);
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdReturn, ref cursor, out requireMoreTokens, out _))
            {
                message = null;
                return false;
            }

            Expr? value = null;
            if (TokenMatch(tokens, false, TokenKind.OptColon, ref cursor, out requireMoreTokens, out _))
            {
                if (!Expr.MatchAny(tokens, true, ref cursor, out requireMoreTokens, out message, out value))
                {
                    if (message == null)
                        message = "Require expression after ':' while parsing 'return-expression'";

                    return false;
                }
            }

            index = cursor;
            expr = new ReturnExpr(value);
            message = null;
            return true;
        }
    }
}
