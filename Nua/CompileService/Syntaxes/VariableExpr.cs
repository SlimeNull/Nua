using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class VariableExpr : ValueExpr, IAssignableSyntax
{
    public string Name { get; }
    public Token? NameToken { get; }

    public VariableExpr(string name)
    {
        Name = name;
    }

    public VariableExpr(Token nameToken)
    {
        if (nameToken.Value is null)
            throw new ArgumentException("Value of name token is null", nameof(nameToken));

        Name = nameToken.Value;
        NameToken = nameToken;
    }


    public override NuaValue? Evaluate(NuaContext context) => context.Get(Name);
    public override CompiledSyntax Compile() => CompiledSyntax.CreateFromDelegate(context => context.Get(Name));

    public void Assign(NuaContext context, NuaValue? newValue)
    {
        context.Set(Name, newValue);
    }
}
