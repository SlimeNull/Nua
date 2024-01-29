﻿using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class CompareExpr : Expr
    {
        public CompareExpr(Expr left, CompareTailExpr tail)
        {
            Left = left;
            Tail = tail;
        }

        public Expr Left { get; }
        public CompareTailExpr Tail { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            return Tail.Evaluate(context, Left);
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!AddExpr.Match(tokens, ref cursor, out var left))
                return false;
            CompareTailExpr.Match(tokens, ref cursor, out var tail);

            index = cursor;
            expr = tail != null ? new CompareExpr(left, tail) : left;
            return true;
        }
    }
}
