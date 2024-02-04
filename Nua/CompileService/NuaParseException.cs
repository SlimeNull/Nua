namespace Nua.CompileService
{
    public class NuaParseException : NuaException
    {
        public NuaParseException()
        {
        }

        public NuaParseException(ParseStatus parseStatus) : base(parseStatus.Message)
        {
            Status = parseStatus;
        }

        public ParseStatus Status { get; }
    }
}
