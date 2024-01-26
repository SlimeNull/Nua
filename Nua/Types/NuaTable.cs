using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nua.Types
{
    public abstract class NuaTable : NuaValue, IEnumerable<KeyValuePair<NuaValue, NuaValue?>>
    {
        public override string TypeName => TableTypeName;

        public abstract void Set(NuaValue key, NuaValue? value);
        public abstract NuaValue? Get(NuaValue key);
        public abstract IEnumerator<KeyValuePair<NuaValue, NuaValue?>> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            return $"{{{string.Join(",", this.Select(kv => $" {kv.Key}: {kv.Value}"))} }}";
        }
    }

}
