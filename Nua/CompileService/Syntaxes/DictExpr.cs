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

        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, ref cursor, TokenKind.BigBracketLeft, out _))
                return false;

            List<DictMemberExpr> members = new();
            while (DictMemberExpr.Match(tokens, ref cursor, out var member))
            {
                members.Add(member);

                if (!TokenMatch(tokens, ref cursor, TokenKind.OptComma, out _))
                    break;
            }

            TokenMatch(tokens, ref cursor, TokenKind.OptComma, out _);

            if (!TokenMatch(tokens, ref cursor, TokenKind.BigBracketRight, out _))
                throw new NuaParseException("Expect '}' after '{' while parsing 'dict-expression'");

            index = cursor;
            expr = new DictExpr(members);
            return true;
        }
    }
}
