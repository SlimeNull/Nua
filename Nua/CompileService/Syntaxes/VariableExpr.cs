using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class VariableExpr : ValueExpr
    {
        public string Name { get; }

        public VariableExpr(string name)
        {
            Name = name;
        }


        public override NuaValue? Eval(NuaContext context) => context.Get(Name);
        public void SetValue(NuaContext context, NuaValue? newValue)
        {
            context.Set(Name, newValue);
        }

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            if (index < 0 || index >= tokens.Count)
                return false;
            if (tokens[index].Kind != TokenKind.Identifier ||
                tokens[index].Value == null)
                return false;

            expr = new VariableExpr(tokens[index].Value!);
            index++;

            return true;
        }
    }
}
