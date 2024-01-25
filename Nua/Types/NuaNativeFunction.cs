
using Nua.CompileService.Syntaxes;

namespace Nua.Types
{
    public class NuaNativeFunction : NuaFunction
    {
        protected readonly string[] _parameterNames;

        public NuaNativeFunction(Expr body, params string[] parameterNames)
        {
            Body = body;
            _parameterNames = parameterNames;
        }

        public Expr Body { get; }

        public override NuaValue? Invoke(NuaContext context, params NuaValue?[] parameters)
        {
            NuaContext localContext = new NuaContext(context);

            for (int i = 0; i < _parameterNames.Length && i < parameters.Length; i++)
                localContext.Set(_parameterNames[i], parameters[i]);

            return Body.Eval(context);
        }
    }
}
