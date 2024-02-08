namespace Nua.CompileService;

public struct LexStatus
{
    public bool HasError => Errors != null ? Errors.Count > 0 : false;
    public List<Error> Errors { get; }

    public record Error(Range TextRange, string Message);

    public LexStatus()
    {
        Errors = new();
    }
}