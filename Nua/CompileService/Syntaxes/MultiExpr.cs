﻿using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class MultiExpr : Syntax
    {
        public MultiExpr(IEnumerable<Expr> expressions)
        {
            Expressions = expressions
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<Expr> Expressions { get; }

        public NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            NuaValue? value = null;
            foreach (var expr in Expressions)
            {
                if (expr is ProcessExpr processExpr)
                {
                    value = processExpr.Evaluate(context, out var subState);

                    if (subState != EvalState.None)
                    {
                        state = subState;
                        return value;
                    }
                }
                else
                {
                    value = expr?.Evaluate(context);
                }
            }

            state = EvalState.None;
            return value;
        }

        public override NuaValue? Evaluate(NuaContext context)
        {
            return Evaluate(context, out _);
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out MultiExpr? expr)
        {
            expr = null;
            List<Expr> expressions = new();

            int cursor = index;
            while (Expr.MatchAny(tokens, ref cursor, out var oneExpr))
                expressions.Add(oneExpr);

            if (expressions.Count == 0)
                return false;

            index = cursor;
            expr = new MultiExpr(expressions);
            return true;
        }
    }
}
