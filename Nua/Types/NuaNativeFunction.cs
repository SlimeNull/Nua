
using Nua.CompileService.Syntaxes;

namespace Nua.Types
{
    public class NuaNativeFunction : NuaFunction
    {
        protected readonly string[] _parameterNames;

        public NuaNativeFunction(MultiExpr? body, params string[] parameterNames)
        {
            Body = body;
            _parameterNames = parameterNames;
        }

        public MultiExpr? Body { get; }

        public override NuaValue? Invoke(NuaContext context, params NuaValue?[] parameters)
        {
            if (Body == null)
                return null;

            NuaContext localContext = new NuaContext(context);

            for (int i = 0; i < _parameterNames.Length && i < parameters.Length; i++)
                localContext.Set(_parameterNames[i], parameters[i]);

            NuaValue? result = null;
            foreach (var expr in Body.Expressions)
            {
                if (expr is ProcessExpr processExpr)
                {
                    result = processExpr.Eval(localContext, out var state);
                    if (state != EvalState.None)
                        break;
                }
                else
                {
                    result = expr.Eval(localContext);
                }
            }

            return result;
        }
    }
}
