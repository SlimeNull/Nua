
namespace Nua.Types
{
    public class NuaNumber : NuaValue, IEquatable<NuaNumber?>
    {
        public NuaNumber(double value)
        {
            Value = value;
        }

        public double Value { get; set; }

        public override bool Equals(object? obj) => Equals(obj as NuaNumber);
        public bool Equals(NuaNumber? other) => other is not null && Value == other.Value;
        public override int GetHashCode() => HashCode.Combine(Value);

        public static bool operator ==(NuaNumber? left, NuaNumber? right) => EqualityComparer<NuaNumber>.Default.Equals(left, right);
        public static bool operator !=(NuaNumber? left, NuaNumber? right) => !(left == right);

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
