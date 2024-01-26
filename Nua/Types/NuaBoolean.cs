namespace Nua.Types
{
    public class NuaBoolean : NuaValue, IEquatable<NuaBoolean?>
    {
        public override string TypeName => BooleanTypeName;
        public bool Value { get; set; }

        public NuaBoolean(bool value)
        {
            Value = value;
        }


        public override bool Equals(object? obj) => Equals(obj as NuaBoolean);
        public bool Equals(NuaBoolean? other) => other is not null && Value == other.Value;
        public override int GetHashCode() => HashCode.Combine(Value);

        public static bool operator ==(NuaBoolean? left, NuaBoolean? right) => EqualityComparer<NuaBoolean>.Default.Equals(left, right);
        public static bool operator !=(NuaBoolean? left, NuaBoolean? right) => !(left == right);

        public override string ToString() => Value.ToString();
    }
}
