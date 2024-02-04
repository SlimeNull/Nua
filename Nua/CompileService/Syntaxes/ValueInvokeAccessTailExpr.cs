using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ValueInvokeAccessTailExpr : ValueAccessTailExpr
    {
        public ValueInvokeAccessTailExpr(IEnumerable<Expr> parameters, ValueAccessTailExpr? nextTail) : base(nextTail)
        {
            Parameters = parameters
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<Expr> Parameters { get; }

        public override NuaValue? Evaluate(NuaContext context, NuaValue? valueToAccess)
        {
            if (valueToAccess == null)
                throw new NuaEvalException("Unable to invoke a null value");

            if (valueToAccess is not NuaFunction function)
                throw new NuaEvalException("Unable to invoke a non-function value");

            NuaValue?[] parameterValues = Parameters
                .Select(p => p.Evaluate(context))
                .ToArray();

            var result = function.Invoke(context, parameterValues);

            if (NextTail != null)
                result = NextTail.Evaluate(context, result);

            return result;
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ValueInvokeAccessTailExpr? expr)
        {
            parseStatus = new();
expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.ParenthesesLeft, ref cursor, out _, out _))
            {
                parseStatus.RequireMoreTokens = false;
                parseStatus.Message = null;
                return false;
            }

            if (!ChainExpr.Match(tokens, true, ref cursor, out var chainParseStatus, out var chain) && chainParseStatus.Intercept)
            {
                parseStatus = chainParseStatus;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.ParenthesesRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                if (chain != null)
                    parseStatus.Message = "Require ')' after '(' while parsing 'value-access-expression'";
                else
                    parseStatus.Message = "Require parameter names after '(' while parsing 'value-access-expression'";

                return false;
            }

            if (!ValueAccessTailExpr.Match(tokens, false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = new ValueInvokeAccessTailExpr(chain?.Expressions ?? [], nextTail);
            parseStatus.Message = null;
            return true;
        }
    }
}
