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

        public NuaValue? Eval(NuaContext context, out bool executed, out EvalState state)
        {
            bool condition = Test(Condition.Eval(context));

            NuaContext ifContext = new NuaContext(context);

            if (condition)
            {
                executed = true;

                if (Body == null)
                {
                    state = EvalState.None;
                    return null;
                }

                return Body.Eval(ifContext, out state);
            }
            else
            {
                if (ElseIfExpressions != null)
                {
                    foreach (var elseif in ElseIfExpressions)
                    {
                        var value = elseif.Eval(ifContext, out var elseIfExecuted, out var elseIfState);

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
                    return ElseExpressions.Eval(ifContext, out state);
                }

                executed = false;
                state = EvalState.None;
                return null;
            }
        }

        public override NuaValue? Eval(NuaContext context, out EvalState state)
        {
            return Eval(context, out _, out state);
        }

        public static bool Test(NuaValue? value)
        {
            if (value == null)
                return false;
            if (value is not NuaBoolean boolean)
                return true;
            else
                return boolean.Value;
        }

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.KwdIf)
                return false;
            cursor++;

            if (!Expr.Match(ExprLevel.All, tokens, ref cursor, out var condition))
                throw new NuaParseException("Require 'if' condition");

            if (cursor < 0 || cursor >= tokens.Count ||
                tokens[cursor].Kind != TokenKind.BigBracketLeft)
                throw new NuaParseException("Require big left bracket after 'if' condition");
            cursor++;


            MultiExpr.Match(tokens, ref cursor, out var body);


            if (cursor < 0 || cursor >= tokens.Count ||
                tokens[cursor].Kind != TokenKind.BigBracketRight)
                throw new NuaParseException("Require bit right bracket after 'if body' expressions");
            cursor++;

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
