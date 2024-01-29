using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public abstract class Syntax
    {
        public abstract NuaValue? Evaluate(NuaContext context);

        protected static bool TokenMatch(IList<Token> tokens, ref int index, TokenKind requiredTokenKind, out Token token)
        {
            token = default;
            if (index < 0 || index >= tokens.Count)
                return false;
            if (tokens[index].Kind != requiredTokenKind)
                return false;

            token = tokens[index];
            index++;
            return true;
        }
    }
}