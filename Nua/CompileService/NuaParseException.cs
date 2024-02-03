namespace Nua.CompileService
{
    public class NuaParseException : NuaException
    {
        public NuaParseException()
        {
        }

        public NuaParseException(bool requireMoreTokens, string? message) : base(message)
        {
            RequireMoreTokens = requireMoreTokens;
        }

        public bool RequireMoreTokens { get; }
    }
}
