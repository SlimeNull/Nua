using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public abstract class Syntax
    {
        public abstract NuaValue? Eval(NuaContext context);
    }
}