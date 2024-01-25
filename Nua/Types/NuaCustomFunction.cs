namespace Nua.Types
{
    public class NuaCustomFunction : NuaFunction
    {
        public NuaCustomFunction(Func<NuaContext, NuaValue?[], NuaValue> handler)
        {
            Handler = handler;
        }

        public Func<NuaContext, NuaValue?[], NuaValue> Handler { get; }

        public override NuaValue? Invoke(NuaContext context, params NuaValue?[] parameters)
            => Handler.Invoke(context, parameters);
    }
}
