namespace Nua.CompileService;

public class NuaParseException : NuaException
{
    public ParseStatus Status { get; }

    public NuaParseException() { }
    public NuaParseException(ParseStatus parseStatus) : base(parseStatus.Message)
    {
        Status = parseStatus;
    }
}
