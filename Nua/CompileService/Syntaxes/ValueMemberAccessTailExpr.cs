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
            if (valueToAccess is not NuaDictionary dict)
                throw new NuaEvalException("Unable to access member of non-dictionary value");

            var key = new NuaString(Name);
            var value = dict.Get(key); 

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
            if (index < 0 || index >= tokens.Count)
                return false;
            if (tokens[index].Kind != TokenKind.OptDot)
                return false;

            int cursor = index;
            cursor++;

            if (tokens[cursor].Kind != TokenKind.Identifier ||
                tokens[cursor].Value == null)
                return false;

            string name = tokens[cursor].Value!;

            cursor++;

            Match(tokens, ref cursor, out var nextTail);

            expr = new ValueMemberAccessTailExpr(name, nextTail);
            index = cursor;
            return true;
        }
    }
}
