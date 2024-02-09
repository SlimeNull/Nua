namespace Nua.Types
{
    public class NuaNativeTable : NuaTable
    {
        public Dictionary<NuaValue, NuaValue> Storage { get; } = new();

        public override void Set(NuaContext context, NuaValue key, NuaValue? value)
        {
            if (Storage.TryGetValue(s_metatableKey, out var metaTableValue) &&
                metaTableValue is NuaTable metaTable &&
                metaTable.Get(context, s_setKey) is NuaFunction newIndexFunc)
            {
                newIndexFunc.Invoke(context, [this, key, value], []);
                return;
            }

            if (value == null)
            {
                Storage.Remove(key);
                return;
            }

            Storage[key] = value;
        }

        public override NuaValue? Get(NuaContext context, NuaValue key)
        {
            if (Storage.TryGetValue(key, out var value))
                return value;

            if (Storage.TryGetValue(s_metatableKey, out var metaTableValue) &&
                metaTableValue is NuaTable metaTable &&
                metaTable.Get(context, s_getKey) is NuaValue indexValue)
            {
                if (indexValue is NuaTable indexTable)
                    return indexTable.Get(context, key);
                else if (indexValue is NuaFunction indexFunc)
                    return indexFunc.Invoke(context, [this, key], []);
            }

            return null;
        }

        public override IEnumerator<KeyValuePair<NuaValue, NuaValue?>> GetEnumerator() => Storage.GetEnumerator();
    }

}
