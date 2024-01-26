namespace Nua.Types
{
    public abstract class NuaValue
    {
        public abstract string TypeName { get; }

        public const string FunctionTypeName = "function";
        public const string TableTypeName = "table";
        public const string ListTypeName = "list";
        public const string BooleanTypeName = "boolean";
        public const string StringTypeName = "string";
        public const string NumberTypeName = "number";
    }
}
