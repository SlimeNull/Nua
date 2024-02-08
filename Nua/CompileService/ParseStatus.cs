namespace Nua.CompileService;

public struct ParseStatus
{
    public bool Intercept;
    public bool RequireMoreTokens;
    public string? Message;
    public Range? ErrorRange;

    public ParseStatus(bool intercept, bool requireMoreTokens, string? message, Range? errorRange)
    {
        Intercept = intercept;
        RequireMoreTokens = requireMoreTokens;
        Message = message;
        ErrorRange = errorRange;
    }
}
