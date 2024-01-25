
namespace Nua.Types
{
    public class NuaString : NuaValue, IEquatable<NuaString?>
    {
        public NuaString(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public override bool Equals(object? obj) => Equals(obj as NuaString);
        public bool Equals(NuaString? other) => other is not null && Value == other.Value;
        public override int GetHashCode() => HashCode.Combine(Value);

        public static bool operator ==(NuaString? left, NuaString? right) => EqualityComparer<NuaString>.Default.Equals(left, right);
        public static bool operator !=(NuaString? left, NuaString? right) => !(left == right);

        public override string ToString()
        {
            return Value;
        }
    }
}
