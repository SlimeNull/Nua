using Nua.CompileService.Syntaxes;

namespace Nua.Types
{
    public class NuaNativeFunction : NuaFunction
    {
        public NuaNativeFunction(MultiExpr? body, params string[] parameterNames)
        {
            Body = body;
            _parameterNames = parameterNames;
        }

        public override IReadOnlyList<string> ParameterNames => _parameterNames.AsReadOnly();

        protected readonly string[] _parameterNames;

        public MultiExpr? Body { get; }

        public override NuaValue? Invoke(NuaContext context, params NuaValue?[] parameters)
        {
            if (Body == null)
                return null;

            context.PushFrame();

            for (int i = 0; i < _parameterNames.Length && i < parameters.Length; i++)
                context.Set(_parameterNames[i], parameters[i]);

            NuaValue? result = null;
            foreach (var expr in Body.Expressions)
            {
                if (expr is ProcessExpr processExpr)
                {
                    result = processExpr.Evaluate(context, out var state);
                    if (state != EvalState.None)
                        break;
                }
                else
                {
                    result = expr.Evaluate(context);
                }
            }

            context.PopFrame();

            return result;
        }
    }
}
