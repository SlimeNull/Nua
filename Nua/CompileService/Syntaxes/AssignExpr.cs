﻿using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class AssignExpr : Expr
    {
        public AssignExpr(Expr left, AssignTailExpr tail)
        {
            Left = left;
            Tail = tail;
        }

        public Expr Left { get; }
        public AssignTailExpr Tail { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            if (Left is ValueAccessExpr valueAccessExpr)
            {
                var value = Tail.Evaluate(context);
                valueAccessExpr.SetMemberValue(context, value);

                return value;
            }
            else if (Left is VariableExpr variableExpr)
            {
                var value = Tail.Evaluate(context);
                variableExpr.SetValue(context, value);

                return value;
            }
            else
            {
                throw new NuaEvalException("Only Value member or Variable can be assigned");
            }
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!PrimaryExpr.Match(tokens, required, ref cursor, out requireMoreTokens, out message, out var left))
                return false;
            if (!AssignTailExpr.Match(tokens, required, ref cursor, out requireMoreTokens, out message, out var tail))
                return false;

            index = cursor;
            expr = new AssignExpr(left, tail);
            return true;
        }
    }
}
