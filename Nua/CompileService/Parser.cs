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
        public static Expr Parse(IList<Token> tokens)
        {
            int cursor = 0;
            if (!Expr.MatchAny(tokens, true, ref cursor, out var requireMoreTokens, out var message, out var expr))
            {
                if (message == null)
                    message = "Invalid expression";

                throw new NuaParseException(requireMoreTokens, message);
            }

            if (cursor < tokens.Count)
                throw new NuaParseException(false, $"Unexpected token '{tokens[cursor]}'");

            return expr;
        }

        public static MultiExpr ParseMulti(IList<Token> tokens)
        {
            int cursor = 0;
            if (!MultiExpr.Match(tokens, true, ref cursor, out var requireMoreTokens, out var message, out var expr))
            {
                if (message == null)
                    message = "Invalid expression";

                throw new NuaParseException(requireMoreTokens, message);
            }
            if (cursor < tokens.Count)
                throw new NuaParseException(false, $"Unexpected token '{tokens[cursor]}'");

            return expr;
        }
    }
}
