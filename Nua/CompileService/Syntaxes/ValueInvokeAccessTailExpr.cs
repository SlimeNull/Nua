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

        public override NuaValue? Eval(NuaContext context, NuaValue valueToAccess)
        {
            if (valueToAccess is not NuaFunction function)
                throw new NuaEvalException("Unable to invoke a non-function value");

            NuaValue?[] parameterValues = Parameters
                .Select(p => p.Eval(context))
                .ToArray();

            return function.Invoke(context, parameterValues);
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ValueInvokeAccessTailExpr? expr)
        {
            expr = null;
            if (index < 0 || index >= tokens.Count ||
                tokens[index].Kind != TokenKind.ParenthesesLeft)
                return false;

            int cursor = index;
            cursor++;

            ChainExpr.Match(tokens, ref cursor, out var chain);

            if (cursor >= tokens.Count ||
                tokens[cursor].Kind != TokenKind.ParenthesesRight)
                return false;
            cursor++;

            ValueAccessTailExpr.Match(tokens, ref cursor, out var nextTail);

            index = cursor;
            expr = new ValueInvokeAccessTailExpr(chain?.Expressions ?? [], nextTail);
            return true;
        }
    }
}
