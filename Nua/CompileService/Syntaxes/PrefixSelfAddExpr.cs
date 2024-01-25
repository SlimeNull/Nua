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

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out PrefixSelfAddExpr? expr)
        {
            expr = null;
            if (index < 0 || index >= tokens.Count)
                return false;
            if (tokens[index].Kind != TokenKind.OptDoubleAdd &&
                tokens[index].Kind != TokenKind.OptDoubleMin)
                return false;

            bool negative = tokens[index].Kind == TokenKind.OptDoubleMin;

            int cursor = index;
            cursor++;


            Expr? self;
            if (ValueAccessExpr.Match(tokens, ref cursor, out var self2))
                self = self2;
            else if (VariableExpr.Match(tokens, ref cursor, out var self1))
                self = self1;
            else
                return false;

            index = cursor;
            expr = new PrefixSelfAddExpr(self, negative);
            return true;
        }
    }
}
