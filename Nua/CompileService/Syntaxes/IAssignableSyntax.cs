using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public interface IAssignableSyntax
{
    public void Assign(NuaContext context, NuaValue? value);
}
