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

        public override NuaValue? Evaluate(NuaContext context)
        {
            if (Self is ValueAccessExpr memberAccessExpr)
            {
                var self = memberAccessExpr.Evaluate(context);

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
                var self = variableExpr.Evaluate(context);

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

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!ValueAccessExpr.Match(tokens, required, ref cursor, out _, out _, out var self))
            {
                requireMoreTokens = false;
                message = null;
                return false;
            }


            Token operatorToken;
            if (!TokenMatch(tokens, false, TokenKind.OptDoubleAdd, ref cursor, out requireMoreTokens, out operatorToken) &&
                !TokenMatch(tokens, false, TokenKind.OptDoubleMin, ref cursor, out requireMoreTokens, out operatorToken))
            {
                index = cursor;
                expr = self;
                message = null;
                return true;
            }

            bool negative = operatorToken.Kind == TokenKind.OptDoubleMin;

            index = cursor;
            expr = new SuffixSelfAddExpr(self, negative);
            message = null;
            return true;
        }
    }
}
