namespace Nua.Types
{
    public abstract class NuaFunction : NuaValue
    {
        public override string TypeName => FunctionTypeName;

        public abstract IReadOnlyList<string> ParameterNames { get; }

        public abstract NuaValue? Invoke(NuaContext context, NuaValue?[] parameters, KeyValuePair<string, NuaValue?>[] namedParameters);

        public override string ToString()
        {
            var parameterNames = ParameterNames;

            if (parameterNames.Count == 0)
                return "<function>";
            else
                return $"<function ({string.Join(", ", parameterNames)})>";
        }
    }
}
