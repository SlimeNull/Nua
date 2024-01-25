using System.Collections;

namespace Nua.Types
{
    public class NuaList : NuaValue
    {
        public List<NuaValue?> Storage { get; } = new();

        public override string ToString()
        {
            return $"[ {string.Join(", ", Storage)} ]";
        }
    }

}
