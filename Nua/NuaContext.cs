using Nua.Types;

namespace Nua
{
    public class NuaContext
    {
        public NuaContext? Parent { get; }
        public Dictionary<string, NuaValue> Values { get; } = new();

        public NuaContext(NuaContext parent)
        {
            Parent = parent;
        }

        public NuaContext()
        {

        }

        public void Set(string name, NuaValue? value)
        {
            if (value == null)
            {
                Values.Remove(name);
                return;
            }

            if (Parent != null)
                Parent.Set(name, value);
            else
                Values[name] = value;
        }

        public NuaValue? Get(string name)
        {
            if (Values.TryGetValue(name, out var value))
                return value;
            if (Parent != null)
                return Parent.Get(name);

            return null;
        }
    }
}
