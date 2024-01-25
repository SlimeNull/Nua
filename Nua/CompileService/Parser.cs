using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nua.CompileService.Syntaxes;

namespace Nua.CompileService
{
    public static class Parser
    {
        public static Expr? Parse(IList<Token> tokens)
        {
            int cursor = 0;
            if (Expr.Match(ExprLevel.All, tokens, ref cursor, out var expr))
                return expr;

            if (cursor < tokens.Count)
                throw new ArgumentException($"Unexpected token '{tokens[cursor]}'");

            return null;
        }
    }
}
