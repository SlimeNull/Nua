using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class DictExpr : ValueExpr
    {
        public IReadOnlyList<DictMemberExpr> Members { get; }

        public DictExpr(IEnumerable<DictMemberExpr> members)
        {
            Members = members
                .ToList()
                .AsReadOnly();
        }

        public override NuaValue? Eval(NuaContext context)
        {
            var dict = new NuaDictionary();

            foreach (var member in Members)
            {
                dict.Set(member.Key.Eval(context)!, member.Value.Eval(context));
            }

            return dict;
        }

        public static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out DictExpr? expr)
        {
            expr = null;
            int cursor = index;

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind != TokenKind.BigBracketLeft)
                return false;
            cursor++;

            List<DictMemberExpr> members = new();

            while (DictMemberExpr.Match(tokens, ref cursor, out var member))
            {
                members.Add(member);

                if (cursor < 0 || cursor >= tokens.Count)
                    break;
                if (tokens[cursor].Kind != TokenKind.OptComma)
                    break;
                cursor++;
            }

            if (cursor < 0 || cursor >= tokens.Count)
                return false;
            if (tokens[cursor].Kind == TokenKind.OptComma)
            {
                cursor++;
                if (cursor < 0 || cursor >= tokens.Count)
                    return false;
            }

            if (tokens[cursor].Kind != TokenKind.BigBracketRight)
                return false;

            index = cursor;
            expr = new DictExpr(members);
            return true;
        }
    }
}
