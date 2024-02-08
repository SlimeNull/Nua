using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ValueInvokeAccessTailExpr : ValueAccessTailExpr
    {
        public IReadOnlyList<Expr> ParameterExpressions { get; }
        public IReadOnlyList<KeyValuePair<string, Expr>> NamedParameterExpressions { get; }

        public ValueInvokeAccessTailExpr(
            IEnumerable<Expr> parameterExpressions,
            IEnumerable<KeyValuePair<string, Expr>> namedParameterExpressions,
            ValueAccessTailExpr? nextTailExpr) : base(nextTailExpr)
        {
            ParameterExpressions = parameterExpressions
                .ToList()
                .AsReadOnly();
            NamedParameterExpressions = namedParameterExpressions
                .ToList()
                .AsReadOnly();
        }

        public override NuaValue? Evaluate(NuaContext context, NuaValue? valueToAccess)
        {
            if (valueToAccess == null)
                throw new NuaEvalException("Unable to invoke a null value");
            if (valueToAccess is not NuaFunction function)
                throw new NuaEvalException("Unable to invoke a non-function value");

            var parameters = ParameterExpressions
                .Select(p => p.Evaluate(context))
                .ToArray();
            var namedParameters = NamedParameterExpressions
                .Select(p => new KeyValuePair<string, NuaValue?>(p.Key, p.Value.Evaluate(context)))
                .ToArray();

            var result = function.Invoke(context, parameters, namedParameters);

            if (NextTailExpr != null)
                result = NextTailExpr.Evaluate(context, result);

            return result;
        }

        public override CompiledSyntax Compile(CompiledSyntax compiledValueToAccess)
        {
            var compiledParameters = ParameterExpressions
                .Select(p => p.Compile())
                .ToArray();
            var compiledNamedParameters = NamedParameterExpressions
                .Select(p => new KeyValuePair<string, CompiledSyntax>(p.Key, p.Value.Compile()))
                .ToArray();

            CompiledSyntax result = CompiledSyntax.CreateFromDelegate(context =>
            {
                var valueToAccess = compiledValueToAccess.Evaluate(context);

                if (valueToAccess == null)
                    throw new NuaEvalException("Unable to invoke a null value");
                if (valueToAccess is not NuaFunction function)
                    throw new NuaEvalException("Unable to invoke a non-function value");

                var parameters = compiledParameters
                    .Select(compiled => compiled.Evaluate(context))
                    .ToArray();
                var namedParameters = compiledNamedParameters
                    .Select(compiled => new KeyValuePair<string, NuaValue?>(compiled.Key, compiled.Value.Evaluate(context)))
                    .ToArray();

                var result = function.Invoke(context, parameters, namedParameters);

                return result;
            });

            if (NextTailExpr is not null)
                result = NextTailExpr.Compile(result);

            return result;
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ValueInvokeAccessTailExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            // 匹配左括号
            if (!TokenMatch(tokens, required, TokenKind.ParenthesesLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Message = null;
                return false;
            }

            List<Expr> positionParams = new();
            List<KeyValuePair<string, Expr>> namedParams = new();
            if (ValueInvokeParameterSyntax.Match(tokens, false, ref cursor, out parseStatus, out var firstParam))
            {
                if (firstParam.Name is not null)
                    namedParams.Add(new KeyValuePair<string, Expr>(firstParam.Name, firstParam.ValueExpr));
                else
                    positionParams.Add(firstParam.ValueExpr);

                while (TokenMatch(tokens, false, TokenKind.OptComma, ref cursor, out parseStatus.RequireMoreTokens, out _))
                {
                    if (!ValueInvokeParameterSyntax.Match(tokens, true, ref cursor, out parseStatus, out var otherParam))
                        return false;

                    if (otherParam.Name is not null)
                    {
                        namedParams.Add(new KeyValuePair<string, Expr>(otherParam.Name, otherParam.ValueExpr));
                    }
                    else
                    {
                        if (namedParams.Count != 0)
                        {
                            parseStatus.Intercept = true;
                            parseStatus.Message = "The named parameter must be after the positional parameter while parsing 'value-invoke-acess-tail-expression'";
                            return false;
                        }

                        positionParams.Add(otherParam.ValueExpr);
                    }
                }
            }

            if (parseStatus.Intercept)
                return false;

            if (!TokenMatch(tokens, true, TokenKind.ParenthesesRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require parameters or ')' after '(' while parsing 'value-access-expression'";
                return false;
            }

            if (!ValueAccessTailExpr.Match(tokens, false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = new ValueInvokeAccessTailExpr(positionParams, namedParams, nextTail);
            parseStatus.Message = null;
            return true;
        }

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;

            foreach (var parameterExpr in ParameterExpressions)
                foreach (var syntax in parameterExpr.TreeEnumerate())
                    yield return syntax;

            if (NextTailExpr is not null)
                foreach (var syntax in NextTailExpr.TreeEnumerate())
                    yield return syntax;
        }
    }
}
