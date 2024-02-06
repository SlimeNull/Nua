using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ElseIfExpr : ProcessExpr
    {
        public Expr ConditionExpr { get; }
        public MultiExpr? BodyExpr { get; }

        public ElseIfExpr(Expr conditionExpr, MultiExpr? bodyExpr)
        {
            ConditionExpr = conditionExpr;
            BodyExpr = bodyExpr;
        }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state) => throw new InvalidOperationException();
        public override CompiledProcessSyntax Compile() => throw new InvalidOperationException();

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ElseIfExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdElif, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = required;
                parseStatus.Message = null;
                return false;
            }

            if (!Expr.Match(tokens, true, ref cursor, out parseStatus, out var condition))
            {
                parseStatus.Intercept = true;
                if (parseStatus.Message == null)
                    parseStatus.Message = "Require 'elif' condition";

                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require big left bracket after 'elif' condition";
                return false;
            }

            if (!MultiExpr.Match(tokens, false, ref cursor, out parseStatus, out var body) && parseStatus.Intercept)
                return false;

            if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require big right bracket after 'elif' condition";
                return false;
            }

            index = cursor;
            expr = new ElseIfExpr(condition, body);
            return true;
        }
        public override IEnumerable<Syntax> TreeEnumerate()
        {

            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;
            foreach (var syntax in ConditionExpr.TreeEnumerate())
                yield return syntax;

            if (BodyExpr is not null)
                foreach (var syntax in BodyExpr.TreeEnumerate())
                    yield return syntax;
        }
    }
}
