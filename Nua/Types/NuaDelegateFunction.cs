namespace Nua.Types
{
    public class NuaDelegateFunction : NuaFunction
    {
        public NuaFunctionHandler Handler { get; }

        public NuaDelegateFunction(NuaFunctionHandler handler)
        {
            Handler = handler;
        }

        public override NuaValue? Invoke(NuaContext context, params NuaValue?[] parameters)
        {
            return Handler.Invoke(context, parameters);
        }

        public delegate NuaValue? NuaFunctionHandler(NuaContext context, NuaValue?[] parameters);
    }
}
