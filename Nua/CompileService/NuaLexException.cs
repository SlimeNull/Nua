namespace Nua.CompileService;

public class NuaLexException : NuaException
{
    public LexStatus Status { get; }

    public NuaLexException() { }
    public NuaLexException(LexStatus status) : base(status.Errors.FirstOrDefault()?.Message)
    {
        Status = status;
    }
}
