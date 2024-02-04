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

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Message = null;
                return false;
            }

            List<TableMemberExpr> members = new();
            while (TableMemberExpr.Match(tokens, false, ref cursor, out parseStatus, out var member))
            {
                members.Add(member);

                if (!TokenMatch(tokens, false, TokenKind.OptComma, ref cursor, out _, out _))
                    break;
            }

            if (parseStatus.Intercept)
                return false;

            // skip the last comma
            TokenMatch(tokens, false, TokenKind.OptComma, ref cursor, out _, out _);

            if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Message = "Expect '}' after '{' while parsing 'table-expression'";
                return false;
            }

            index = cursor;
            expr = new TableExpr(members);
            parseStatus.Message = null;
            return true;
        }
    }
}
