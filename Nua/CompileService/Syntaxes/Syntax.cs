using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public abstract class Syntax
    {
        public abstract NuaValue? Evaluate(NuaContext context);
        public abstract CompiledSyntax Compile();

        protected static bool TokenMatch(IList<Token> tokens, bool required, TokenKind requiredTokenKind, ref int index, out bool requireToken, out Token token)
        {
            token = default;
            if (index < 0 || index >= tokens.Count)
            {
                requireToken = required;
                return false;
            }

            if (tokens[index].Kind != requiredTokenKind)
            {
                requireToken = false;
                return false;
            }

            token = tokens[index];
            index++;
            requireToken = false;

            return true;
        }

        public virtual IEnumerable<Syntax> TreeEnumerate()
        {
            yield return this;
        }
    }
}