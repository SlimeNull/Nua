namespace Nua.Types
{
    public class NuaNativeTable : NuaTable
    {
        public Dictionary<NuaValue, NuaValue> Storage { get; } = new();

        public override void Set(NuaValue key, NuaValue? value)
        {
            if (value == null)
            {
                Storage.Remove(key);
                return;
            }

            Storage[key] = value;
        }

        public override NuaValue? Get(NuaValue key) 
            => Storage.TryGetValue(key, out var value) ? value : null;

        public override IEnumerator<KeyValuePair<NuaValue, NuaValue?>> GetEnumerator() => Storage.GetEnumerator();
    }

}
