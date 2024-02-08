using Nua.CompileService.Syntaxes;
using Nua.Types;

namespace Nua.CompileService;

public abstract class CompiledProcessSyntax : CompiledSyntax
{
    public abstract NuaValue? Evaluate(NuaContext context, out EvalState state);

    public static CompiledProcessSyntax Create(CompiledSyntax? value, EvalState state)
    {
        return new StaticCompiledProcessSyntax(value, state);
    }
    public static CompiledProcessSyntax CreateFromDelegate(ImplementationDelegate implementation)
    {
        return new DelegateCompiledProcessSyntax(implementation);
    }



    public new delegate NuaValue? ImplementationDelegate(NuaContext context, out EvalState staet);

    class StaticCompiledProcessSyntax : CompiledProcessSyntax
    {
        public StaticCompiledProcessSyntax(CompiledSyntax? value, EvalState state)
        {
            ValueSyntax = value;
            State = state;
        }

        public CompiledSyntax? ValueSyntax { get; }
        public EvalState State { get; }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            state = State;
            return ValueSyntax?.Evaluate(context);
        }

        public override NuaValue? Evaluate(NuaContext context) => ValueSyntax?.Evaluate(context);
    }

    class DelegateCompiledProcessSyntax : CompiledProcessSyntax
    {
        private readonly ImplementationDelegate _impl;

        public DelegateCompiledProcessSyntax(ImplementationDelegate impl)
        {
            _impl = impl;
        }

        public override NuaValue? Evaluate(NuaContext context) => _impl.Invoke(context, out _);
        public override NuaValue? Evaluate(NuaContext context, out EvalState state) => _impl.Invoke(context, out state);
    }
}
