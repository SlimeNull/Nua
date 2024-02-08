
namespace Nua.Types
{
    public class NuaDelegateFunction : NuaFunction
    {
        private readonly string[] _parameterNames;

        public NuaFunctionHandler Handler { get; }

        public override IReadOnlyList<string> ParameterNames => _parameterNames.AsReadOnly();

        public NuaDelegateFunction(NuaFunctionHandler handler, params string[] parameterNames)
        {
            Handler = handler;
            _parameterNames = parameterNames;
        }

        public override NuaValue? Invoke(NuaContext context, NuaValue?[] parameters, KeyValuePair<string, NuaValue?>[] namedParameters)
        {
            return Handler.Invoke(context, parameters);
        }

        public delegate NuaValue? NuaFunctionHandler(NuaContext context, NuaValue?[] parameters);
    }
}
