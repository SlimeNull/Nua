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

        public override NuaValue? Eval(NuaContext context, NuaValue valueToAccess)
        {
            if (valueToAccess is not NuaTable table)
                throw new NuaEvalException("Unable to access member of non-table value");

            var key = new NuaString(Name);
            var value = table.Get(key); 

            if (NextTail != null)
            {
                if (value == null)
                    throw new NuaEvalException("Unable to access member of null value");

                value = NextTail.Eval(context, value);
            }

            return value;
        }

        public override NuaValue? Eval(NuaContext context) => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out ValueMemberAccessTailExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, ref cursor, TokenKind.OptDot, out _))
                return false;

            if (!TokenMatch(tokens, ref cursor, TokenKind.Identifier, out var idToken))
                throw new NuaParseException("Require identifier after '.' while parsing 'value-access-expression'");

            string name = idToken.Value!;

            Match(tokens, ref cursor, out var nextTail);

            expr = new ValueMemberAccessTailExpr(name, nextTail);
            index = cursor;
            return true;
        }
    }
}
