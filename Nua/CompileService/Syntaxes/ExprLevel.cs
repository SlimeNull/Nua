namespace Nua.CompileService.Syntaxes
{
    [Flags]
    public enum ExprLevel : int
    {
        None     = 0,
        Value    = 1 << 0,
        Primary  = 1 << 1, 
        Unary    = 1 << 2,
        Process  = 1 << 3,
        Mul      = 1 << 4, 
        Add      = 1 << 5,
        Compare  = 1 << 6,
        Equal    = 1 << 7,
        And      = 1 << 8,
        Or       = 1 << 9, 
        Assign   = 1 << 10,
        All = int.MaxValue
    }
}
