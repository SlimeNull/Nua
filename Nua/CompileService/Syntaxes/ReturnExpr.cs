using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public class ReturnExpr : ProcessExpr
{
    public Expr? ValueExpr { get; }

    public ReturnExpr(Expr? valueExpr)
    {
        ValueExpr = valueExpr;
    }

    public override NuaValue? Evaluate(NuaContext context, out EvalState state)
    {
        state = EvalState.Return;
        return ValueExpr?.Evaluate(context);
    }

    public override CompiledProcessSyntax Compile()
    {
        return CompiledProcessSyntax.Create(ValueExpr?.Compile(), EvalState.Return);
    }

}
