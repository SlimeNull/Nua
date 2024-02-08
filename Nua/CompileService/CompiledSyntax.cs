using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nua.Types;

namespace Nua.CompileService;

public abstract class CompiledSyntax
{
    public abstract NuaValue? Evaluate(NuaContext context);


    public static CompiledSyntax Empty { get; } = new StaticCompiledSyntax(null);

    public static CompiledSyntax Create(NuaValue? value) => new StaticCompiledSyntax(value);
    public static CompiledSyntax CreateFromDelegate(ImplementationDelegate implementation)
    {
        return new DelegateCompiledSyntax(implementation);
    }


    public delegate NuaValue? ImplementationDelegate(NuaContext context);

    class StaticCompiledSyntax : CompiledSyntax
    {
        public NuaValue? Value { get; }

        public StaticCompiledSyntax(NuaValue? value)
        {
            Value = value;
        }

        public override NuaValue? Evaluate(NuaContext context) => Value;
    }

    class DelegateCompiledSyntax : CompiledSyntax
    {
        private readonly ImplementationDelegate _impl;

        public DelegateCompiledSyntax(ImplementationDelegate impl)
        {
            _impl = impl;
        }

        public override NuaValue? Evaluate(NuaContext context) => _impl.Invoke(context);
    }
}
