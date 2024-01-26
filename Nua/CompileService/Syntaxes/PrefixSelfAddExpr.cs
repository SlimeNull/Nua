using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class PrefixSelfAddExpr : UnaryExpr
    {
        public PrefixSelfAddExpr(Expr self, bool negative)
        {
            Self = self;
            Negative = negative;
        }

        public Expr Self { get; }
        public bool Negative { get; }

        public override NuaValue? Eval(NuaContext context)
        {
            if (Self is ValueAccessExpr memberAccessExpr)
            {
                var self = memberAccessExpr.Eval(context);

                if (self == null)
                    throw new NuaEvalException("Unable to apply self-increment on a null value");
                if (self is not NuaNumber number)
                    throw new NuaEvalException("Unable to apply self-increment on a non-number value");

                var newValue = !Negative ?
                    new NuaNumber(number.Value + 1) :
                    new NuaNumber(number.Value - 1);

                memberAccessExpr.SetMemberValue(context, newValue);
                return newValue;
            }
            else if (Self is VariableExpr variableExpr)
            {
                var self = variableExpr.Eval(context);

                if (self == null)
                    throw new NuaEvalException("Unable to apply self-increment on a null value");
                if (self is not NuaNumber number)
                    throw new NuaEvalException("Unable to apply self-increment on a non-number value");

                var newValue = !Negative ?
                    new NuaNumber(number.Value + 1) :
                    new NuaNumber(number.Value - 1);

                variableExpr.SetValue(context, newValue);
                return newValue;
            }
            else
            {
                throw new NuaEvalException("Only Dictionary member and Variable can be increased");
            }
        }

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            Token operatorToken;
            if (!TokenMatch(tokens, ref index, TokenKind.OptDoubleAdd, out operatorToken) &&
                !TokenMatch(tokens, ref index, TokenKind.OptDoubleMin, out operatorToken))
                return false;

            bool negative = operatorToken.Kind == TokenKind.OptDoubleMin;


            Expr? self;
            if (ValueAccessExpr.Match(tokens, ref cursor, out var self2))
                self = self2;
            else if (VariableExpr.Match(tokens, ref cursor, out var self1))
                self = self1;
            else
                throw new NuaParseException("Expect 'value-access-expressoin' or 'variable-expression' after '++' or '--' while parsing 'prefix-self-add-expression'");

            index = cursor;
            expr = new PrefixSelfAddExpr(self, negative);
            return true;
        }
    }
}
