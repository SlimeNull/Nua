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
            if (!Expr.MatchAny(tokens, true, ref cursor, out var parseStatus, out var expr))
            {
                if (parseStatus.Message == null)
                    parseStatus.Message = "Invalid expression";

                throw new NuaParseException(parseStatus);
            }

            if (cursor < tokens.Count)
                throw new NuaParseException(new ParseStatus(false, false, $"Unexpected token '{tokens[cursor]}'"));

            return expr;
        }

        public static MultiExpr ParseMulti(IList<Token> tokens)
        {
            int cursor = 0;
            if (!MultiExpr.Match(tokens, true, ref cursor, out var parseStatus, out var expr))
            {
                if (parseStatus.Message == null)
                    parseStatus.Message = "Invalid expression";

                throw new NuaParseException(parseStatus);
            }
            if (cursor < tokens.Count)
                throw new NuaParseException(new ParseStatus(false, false, $"Unexpected token '{tokens[cursor]}'"));

            return expr;
        }
    }
}
