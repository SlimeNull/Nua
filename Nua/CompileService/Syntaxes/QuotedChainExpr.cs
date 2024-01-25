﻿using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class QuotedChainExpr : Syntax
    {
        public QuotedChainExpr(ChainExpr chain)
        {
            Chain = chain;
        }

        public ChainExpr Chain { get; }

        public override NuaValue? Eval(NuaContext context) => Chain.Eval(context);

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out QuotedChainExpr? expr)
        {
            expr = null;
            if (index < 0 || index >= tokens.Count)
                return false;
            if (tokens[index].Kind != TokenKind.ParenthesesLeft)
                return false;

            int cursor = index;
            cursor++;

            if (!ChainExpr.Match(tokens, ref cursor, out var chain))
                return false;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.ParenthesesRight)
                return false;
            cursor++;

            index = cursor;
            expr = new QuotedChainExpr(chain);
            return true;
        }   
    }
}
