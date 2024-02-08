namespace Nua.CompileService;

public struct ParseStatus
{
    public bool Intercept;
    public bool RequireMoreTokens;
    public string? Message;

    public ParseStatus(bool intercept, bool requireMoreTokens, string? message)
    {
        Intercept = intercept;
        RequireMoreTokens = requireMoreTokens;
        Message = message;
    }
}
