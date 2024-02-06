using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Xml.Linq;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class ValueMemberAccessTailExpr : ValueAccessTailExpr
    {
        public string Name { get; }
        public Token? NameToken { get; }

        public ValueMemberAccessTailExpr(string name, ValueAccessTailExpr? nextTailExpr) : base(nextTailExpr)
        {
            Name = name;
        }

        public ValueMemberAccessTailExpr(Token nameToken, ValueAccessTailExpr? nextTailExpr) : base(nextTailExpr)
        {
            if (nameToken.Value is null)
                throw new ArgumentException("Value of name token is null", nameof(nameToken));

            Name = nameToken.Value;
            NameToken = nameToken;
        }

        public override NuaValue? Evaluate(NuaContext context, NuaValue? valueToAccess)
        {
            if (valueToAccess == null)
                throw new NuaEvalException("Unable to access member of a null value");
            if (valueToAccess is not NuaTable table)
                throw new NuaEvalException("Unable to access member of non-table value");

            var key = new NuaString(Name);
            var value = table.Get(key);

            if (NextTailExpr != null)
                value = NextTailExpr.Evaluate(context, value);

            return value;
        }

        public override CompiledSyntax Compile(CompiledSyntax compiledValueToAccess)
        {
            CompiledSyntax result = CompiledSyntax.CreateFromDelegate(context =>
            {
                var valueToAccess = compiledValueToAccess.Evaluate(context);

                if (valueToAccess == null)
                    throw new NuaEvalException("Unable to access member of a null value");
                if (valueToAccess is not NuaTable table)
                    throw new NuaEvalException("Unable to access member of non-table value");

                var key = new NuaString(Name);
                var value = table.Get(key);

                return value;
            });

            if (NextTailExpr is not null)
                result = NextTailExpr.Compile(result);

            return result;
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ValueMemberAccessTailExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.OptDot, ref cursor, out _, out _))
            {
                parseStatus.RequireMoreTokens = false;
                parseStatus.Message = null;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.Identifier, ref cursor, out parseStatus.RequireMoreTokens, out var idToken))
            {
                parseStatus.Message = "Require identifier after '.' while parsing 'value-access-expression'";
                return false;
            }

            if (!ValueAccessTailExpr.Match(tokens, false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            expr = new ValueMemberAccessTailExpr(idToken, nextTail);
            index = cursor;
            parseStatus.Message = null;
            return true;
        }

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;

            if (NextTailExpr is not null)
                foreach (var syntax in NextTailExpr.TreeEnumerate())
                    yield return syntax;
        }
    }
}
