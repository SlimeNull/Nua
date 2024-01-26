using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nua.Types
{
    public class NuaTable : NuaValue
    {
        public Dictionary<NuaValue, NuaValue> Storage { get; } = new();

        public void Set(NuaValue key, NuaValue? value)
        {
            if (value == null)
            {
                Storage.Remove(key);
                return;
            }

            Storage[key] = value;
        }

        public NuaValue? Get(NuaValue key) 
            => Storage.TryGetValue(key, out var value) ? value : null;

        public override string ToString()
        {
            return $"{{ {string.Join(", ", Storage.Select(kv => $"{kv.Key}: {kv.Value}"))} }}";
        }
    }

}
