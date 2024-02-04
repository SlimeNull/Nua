using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class GlobalExpr : ProcessExpr
    {
        public GlobalExpr(string variableName)
        {
            VariableName = variableName;
        }

        public string VariableName { get; }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            context.TagGlobal(VariableName);
            state = EvalState.None;
            return context.Get(VariableName);
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdGlobal, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = required;
                return false;
            }

            if (!TokenMatch(tokens, true, TokenKind.Identifier, ref cursor, out parseStatus.RequireMoreTokens, out var variableNameToken))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require identifier after 'global' keyword while parsing 'global-expression";
                return false;
            }

            index = cursor;
            expr = new GlobalExpr(variableNameToken.Value!);
            return true;
        }
    }
}
