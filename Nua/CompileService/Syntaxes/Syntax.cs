using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes;

public abstract class Syntax
{
    public virtual IEnumerable<Syntax> TreeEnumerate()
    {
        yield return this;
    }
}