using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Xml.Linq;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class ValueMemberAccessTailExpr : ValueAccessTailExpr
    {
        public ValueMemberAccessTailExpr(string name, ValueAccessTailExpr? nextTail) : base(nextTail)
        {
            Name = name;
        }

        public string Name { get; }

        public override NuaValue? Evaluate(NuaContext context, NuaValue? valueToAccess)
        {
            if (valueToAccess == null)
                throw new NuaEvalException("Unable to access member of a null value");

            if (valueToAccess is not NuaTable table)
                throw new NuaEvalException("Unable to access member of non-table value");

            var key = new NuaString(Name);
            var value = table.Get(key); 

            if (NextTail != null)
                value = NextTail.Evaluate(context, value);

            return value;
        }

        public override NuaValue? Evaluate(NuaContext context) => throw new InvalidOperationException();

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

            string name = idToken.Value!;

            if (!ValueAccessTailExpr.Match(tokens, false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            expr = new ValueMemberAccessTailExpr(name, nextTail);
            index = cursor;
            parseStatus.Message = null;
            return true;
        }
    }
}
