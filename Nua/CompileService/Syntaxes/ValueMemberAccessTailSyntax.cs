using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Xml.Linq;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ValueMemberAccessTailSyntax : ValueAccessTailSyntax
{
    public string Name { get; }
    public Token? NameToken { get; }

    public ValueMemberAccessTailSyntax(string name)
    {
        Name = name;
    }

    public ValueMemberAccessTailSyntax(Token nameToken)
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
        var value = table.Get(context, key);

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
            var value = table.Get(context, key);

            return value;
        });

        return result;
    }

    public override IEnumerable<Syntax> TreeEnumerate()
    {
        foreach (var syntax in base.TreeEnumerate())
            yield return syntax;
    }
}
