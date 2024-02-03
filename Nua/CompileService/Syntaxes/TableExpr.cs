using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class TableExpr : ValueExpr
    {
        public IReadOnlyList<TableMemberExpr> Members { get; }

        public TableExpr(IEnumerable<TableMemberExpr> members)
        {
            Members = members
                .ToList()
                .AsReadOnly();
        }

        public override NuaValue? Evaluate(NuaContext context)
        {
            var table = new NuaNativeTable();

            foreach (var member in Members)
            {
                table.Set(member.Key.Evaluate(context)!, member.Value.Evaluate(context));
            }

            return table;
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.BigBracketLeft, ref cursor, out requireMoreTokens, out _))
            {
                message = null;
                return false;
            }

            List<TableMemberExpr> members = new();
            while (TableMemberExpr.Match(tokens, false, ref cursor, out requireMoreTokens, out message, out var member))
            {
                members.Add(member);

                if (!TokenMatch(tokens, false, TokenKind.OptComma, ref cursor, out _, out _))
                    break;
            }

            if (requireMoreTokens)
                return false;

            // skip the last comma
            TokenMatch(tokens, false, TokenKind.OptComma, ref cursor, out _, out _);

            if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out requireMoreTokens, out _))
            {
                message = "Expect '}' after '{' while parsing 'table-expression'";
                return false;
            }

            index = cursor;
            expr = new TableExpr(members);
            message = null;
            return true;
        }
    }
}
