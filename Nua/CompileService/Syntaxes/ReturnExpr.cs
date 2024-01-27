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

        public override NuaValue? Eval(NuaContext context, out EvalState state)
        {
            state = EvalState.Return;
            return Value?.Eval(context);
        }

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, ref cursor, TokenKind.KwdReturn, out _))
                return false;

            Expr? value = null;
            if (TokenMatch(tokens, ref cursor, TokenKind.OptColon, out _))
            {
                if (!Expr.MatchAny(tokens, ref cursor, out value))
                    throw new NuaParseException("Require expression after ':' while parsing 'return-expression'");
            }

            index = cursor;
            expr = new ReturnExpr(value);
            return true;
        }
    }
}
