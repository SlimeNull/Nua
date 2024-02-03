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

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out ValueInvokeAccessTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.ParenthesesLeft, ref cursor, out _, out _))
            {
                requireMoreTokens = false;
                message = null;
                return false;
            }

            if (!ChainExpr.Match(tokens, true, ref cursor, out var chainRequireMoreTokens, out var chainMessage, out var chain) && chainRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = chainMessage;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.ParenthesesRight, ref cursor, out requireMoreTokens, out _))
            {
                if (chain != null)
                    message = "Require ')' after '(' while parsing 'value-access-expression'";
                else
                    message = "Require parameter names after '(' while parsing 'value-access-expression'";

                return false;
            }

            if (!ValueAccessTailExpr.Match(tokens, false, ref cursor, out var tailRequireMoreTokens, out var tailMessage, out var nextTail) && tailRequireMoreTokens)
            {
                requireMoreTokens = true;
                message = tailMessage;
                return false;
            }

            index = cursor;
            expr = new ValueInvokeAccessTailExpr(chain?.Expressions ?? [], nextTail);
            message = null;
            return true;
        }
    }
}
