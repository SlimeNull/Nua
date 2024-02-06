
using Nua.CompileService;

namespace Nua.Types
{
    public class NuaCompiledNativeFunction : NuaFunction
    {
        public NuaCompiledNativeFunction(CompiledProcessSyntax? body, params string[] parameterNames)
        {
            Body = body;
            _parameterNames = parameterNames;
        }

        public CompiledProcessSyntax? Body { get; }
        public override IReadOnlyList<string> ParameterNames => _parameterNames.AsReadOnly();


        protected readonly string[] _parameterNames;

        public override NuaValue? Invoke(NuaContext context, params NuaValue?[] parameters)
        {
            if (Body == null)
                return null;

            context.PushFrame();

            for (int i = 0; i < _parameterNames.Length && i < parameters.Length; i++)
                context.Set(_parameterNames[i], parameters[i]);

            NuaValue? result = null;
            result = Body?.Evaluate(context, out var state);

            context.PopFrame();

            return result;
        }
    }
}
