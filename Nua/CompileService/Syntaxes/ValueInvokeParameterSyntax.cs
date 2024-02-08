using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes
{
    public class ValueInvokeParameterSyntax : Syntax
    {
        public ValueInvokeParameterSyntax(Expr valueExpr, string? name)
        {
            ValueExpr = valueExpr;
            Name = name;
        }

        public Expr ValueExpr { get; }
        public string? Name { get; }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ValueInvokeParameterSyntax? syntax)
        {
            syntax = null;
            parseStatus = new();
            int cursor = index;

            bool hasName =
                TokenMatch(tokens, false, TokenKind.Identifier, ref cursor, out _, out var nameToken) &&
                TokenMatch(tokens, false, TokenKind.OptColon, ref cursor, out _, out _);

            if (!hasName)
                cursor = index;

            if (!Expr.Match(tokens, required, ref cursor, out parseStatus, out var valueExpr))
                return false;

            index = cursor;
            syntax = new ValueInvokeParameterSyntax(valueExpr, hasName ? nameToken.Value! : null);
            return true;
        }
    }
}
