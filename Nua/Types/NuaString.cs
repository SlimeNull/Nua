
namespace Nua.Types
{
    public class NuaString : NuaValue, IEquatable<NuaString?>
    {
        public override string TypeName => StringTypeName;
        public string Value { get; set; }

        public NuaString(string value)
        {
            Value = value;
        }


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
