namespace Nua.CompileService.Syntaxes
{
    public enum ExprLevel : int
    {
        Value, Primary, Unary, Process, Mul, Add, Compare, Equal, And, Or, Assign,
        All = int.MaxValue
    }
}
