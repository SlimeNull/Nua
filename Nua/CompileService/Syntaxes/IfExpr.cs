using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class IfExpr : ProcessExpr
    {
        public IfExpr(Expr condition, MultiExpr? body, IEnumerable<ElseIfExpr>? elseIfExpressions, ElseExpr? elseExpressions)
        {
            Condition = condition;
            Body = body;
            ElseIfExpressions = elseIfExpressions?.ToList()?.AsReadOnly();
            ElseExpressions = elseExpressions;
        }

        public Expr Condition { get; }
        public MultiExpr? Body { get; }
        public IReadOnlyList<ElseIfExpr>? ElseIfExpressions { get; }
        public ElseExpr? ElseExpressions { get; }

        public NuaValue? Evaluate(NuaContext context, out bool executed, out EvalState state)
        {
            bool condition = NuaUtilities.ConditionTest(Condition.Evaluate(context));

            NuaContext ifContext = new NuaContext(context);

            if (condition)
            {
                executed = true;

                if (Body == null)
                {
                    state = EvalState.None;
                    return null;
                }

                return Body.Evaluate(ifContext, out state);
            }
            else
            {
                if (ElseIfExpressions != null)
                {
                    foreach (var elseif in ElseIfExpressions)
                    {
                        var value = elseif.Evaluate(ifContext, out var elseIfExecuted, out var elseIfState);

                        if (elseIfExecuted)
                        {
                            executed = true;
                            state = elseIfState;
                            return value;
                        }
                    }
                }

                if (ElseExpressions != null)
                {
                    executed = true;
                    return ElseExpressions.Evaluate(ifContext, out state);
                }

                executed = false;
                state = EvalState.None;
                return null;
            }
        }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            return Evaluate(context, out _, out state);
        }

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, ref cursor, TokenKind.KwdIf, out _))
                return false;

            if (!Expr.MatchAny(tokens, ref cursor, out var condition))
                throw new NuaParseException("Require 'if' condition");

            if (!TokenMatch(tokens, ref cursor, TokenKind.BigBracketLeft, out _))
                throw new NuaParseException("Require big left bracket after 'if' condition");

            MultiExpr.Match(tokens, ref cursor, out var body);

            if (!TokenMatch(tokens, ref cursor, TokenKind.BigBracketRight, out _))
                throw new NuaParseException("Require bit right bracket after 'if body' expressions");

            List<ElseIfExpr>? elseifs = null;

            while (ElseIfExpr.Match(tokens, ref cursor, out var elseif))
            {
                if (elseifs == null)
                    elseifs = new();

                elseifs.Add(elseif);
            }

            ElseExpr.Match(tokens, ref cursor, out var elseExpr);

            index = cursor;
            expr = new IfExpr(condition, body, elseifs, elseExpr);
            return true;
        }
    }
}
