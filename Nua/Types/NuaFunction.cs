namespace Nua.Types
{
    public abstract class NuaFunction : NuaValue
    {
        public abstract NuaValue? Invoke(NuaContext context, params NuaValue?[] parameters);

        public override string ToString()
        {
            return "<function>";
        }
    }
}
