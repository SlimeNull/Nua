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

        bool Modify(string name, NuaValue? value)
        {
            if (Parent != null && Parent.Modify(name, value))
                return true;

            if (Values.ContainsKey(name))
            {
                if (value == null)
                    Values.Remove(name);
                else
                    Values[name] = value;

                return true;
            }

            return false;
        }

        public void Set(string name, NuaValue? value)
        {
            if (!Modify(name, value))
            {
                if (value == null)
                    Values.Remove(name);
                else
                    Values[name] = value;
            }
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
