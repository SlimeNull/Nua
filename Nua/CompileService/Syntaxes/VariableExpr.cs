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

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            parseStatus.RequireMoreTokens = required;
            parseStatus.Message = null;


            if (!TokenMatch(tokens, required, TokenKind.Identifier, ref index, out _, out var idToken))
                return false;

            expr = new VariableExpr(idToken.Value!);
            return true;
        }
    }
}
