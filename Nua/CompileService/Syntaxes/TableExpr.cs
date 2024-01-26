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

        public override NuaValue? Eval(NuaContext context)
        {
            var table = new NuaTable();

            foreach (var member in Members)
            {
                table.Set(member.Key.Eval(context)!, member.Value.Eval(context));
            }

            return table;
        }

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, ref cursor, TokenKind.BigBracketLeft, out _))
                return false;

            List<TableMemberExpr> members = new();
            while (TableMemberExpr.Match(tokens, ref cursor, out var member))
            {
                members.Add(member);

                if (!TokenMatch(tokens, ref cursor, TokenKind.OptComma, out _))
                    break;
            }

            TokenMatch(tokens, ref cursor, TokenKind.OptComma, out _);

            if (!TokenMatch(tokens, ref cursor, TokenKind.BigBracketRight, out _))
                throw new NuaParseException("Expect '}' after '{' while parsing 'table-expression'");

            index = cursor;
            expr = new TableExpr(members);
            return true;
        }
    }
}
