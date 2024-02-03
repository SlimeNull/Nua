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


        public override NuaValue? Evaluate(NuaContext context) => context.Get(Name);
        public void SetValue(NuaContext context, NuaValue? newValue)
        {
            context.Set(Name, newValue);
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            requireMoreTokens = required;
            message = null;


            if (!TokenMatch(tokens, required, TokenKind.Identifier, ref index, out _, out var idToken))
                return false;

            expr = new VariableExpr(idToken.Value!);
            return true;
        }
    }
}
