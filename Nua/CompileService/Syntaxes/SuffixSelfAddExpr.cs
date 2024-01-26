using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class SuffixSelfAddExpr : PrimaryExpr
    {
        public SuffixSelfAddExpr(Expr self, bool negative)
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
                return self;
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
                return self;
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

            Expr? self;
            if (ValueAccessExpr.Match(tokens, ref cursor, out var expr2))
                self = expr2;
            else if (VariableExpr.Match(tokens, ref cursor, out var expr1))
                self = expr1;
            else
                return false;


            Token operatorToken;
            if (!TokenMatch(tokens, ref cursor, TokenKind.OptDoubleAdd, out operatorToken) &&
                !TokenMatch(tokens, ref cursor, TokenKind.OptDoubleMin, out operatorToken))
            {
                expr = self;
                return true;
            }

            bool negative = operatorToken.Kind == TokenKind.OptDoubleMin;

            index = cursor;
            expr = new SuffixSelfAddExpr(self, negative);
            return true;
        }
    }
}
