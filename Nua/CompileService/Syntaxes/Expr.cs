using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Nua.CompileService.Syntaxes
{

    public abstract class Expr : Syntax
    {
        public static bool MatchAny(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            return
                AssignExpr.Match(tokens, ref index, out expr) ||
                OrExpr.Match(tokens, ref index, out expr) ||
                AndExpr.Match(tokens, ref index, out expr) ||
                EqualExpr.Match(tokens, ref index, out expr) ||
                CompareExpr.Match(tokens, ref index, out expr) ||
                AddExpr.Match(tokens, ref index, out expr) ||
                MulExpr.Match(tokens, ref index, out expr) ||
                ProcessExpr.Match(tokens, ref index, out expr) ||
                UnaryExpr.Match(tokens, ref index, out expr) ||
                PrimaryExpr.Match(tokens, ref index, out expr) ||
                ValueExpr.Match(tokens, ref index, out expr);
        }
    }
}
