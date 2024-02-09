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
        protected static readonly NuaValue s_metatableKey = new NuaString("__meta_table");

        protected static readonly NuaValue s_getKey = new NuaString("__get");
        protected static readonly NuaValue s_setKey = new NuaString("__set");
        protected static readonly NuaValue s_invokeKey = new NuaString("__invoke");

        public override string TypeName => TableTypeName;

        public abstract void Set(NuaContext context, NuaValue key, NuaValue? value);
        public abstract NuaValue? Get(NuaContext context, NuaValue key);
        public NuaFunction? GetInvocationFunction(NuaContext context)
        {
            if (Get(context, s_metatableKey) is not NuaTable metaTable ||
                metaTable.Get(context, s_invokeKey) is not NuaFunction invocationFunction)
                return null;

            return invocationFunction;
        }

        public abstract IEnumerator<KeyValuePair<NuaValue, NuaValue?>> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            return $"{{{string.Join(",", this.Select(kv => $" {kv.Key}: {kv.Value}"))} }}";
        }
    }

}
