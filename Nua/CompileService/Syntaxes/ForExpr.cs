using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes
{
    public abstract class ForExpr : ProcessExpr
    {
        public new static bool Match(IList<Token> tokens, ref int index, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (index < 0 || index >= tokens.Count ||
                tokens[index].Kind != TokenKind.KwdFor)
                return false;
            cursor++;

            VariableExpr valueNameExpr;
            if (!VariableExpr.Match(tokens, ref cursor, out var _valueNameExpr))
                throw new NuaParseException("Require variable after 'for' keyword");
            valueNameExpr = (VariableExpr)_valueNameExpr;

            if (cursor >= tokens.Count)
                throw new NuaParseException("Require 'in' or 'of' after variable name in 'for' expression");

            VariableExpr? keyNameExpr = null;
            if (tokens[cursor].Kind == TokenKind.OptComma)
            {
                cursor++;
                if (!VariableExpr.Match(tokens, ref cursor, out var _keyNameExpr))
                    throw new NuaParseException("Require variable after comma in 'for' expression");
                keyNameExpr = (VariableExpr)_keyNameExpr;

                if (cursor >= tokens.Count)
                    throw new NuaParseException("Require 'in' or 'of' after variable name in 'for' expression");
            }

            ForOperation operation = tokens[cursor].Kind switch
            {
                TokenKind.KwdIn => ForOperation.In,
                TokenKind.KwdOf => ForOperation.Of,
                _ => throw new NuaParseException("Require 'in' or 'of' after variable name in 'for' expression")
            };
            cursor++;

            if (operation == ForOperation.In)
            {
                if (!Expr.Match(ExprLevel.All, tokens, ref cursor, out var iterable))
                    throw new NuaParseException("Require iterable expression after 'in' keyword of 'for' expression");

                if (cursor < 0 || cursor >= tokens.Count ||
                    tokens[cursor].Kind != TokenKind.BigBracketLeft)
                    throw new NuaParseException("Require big left bracket after 'for' iterable expression");
                cursor++;

                if (!MultiExpr.Match(tokens, ref cursor, out var body))
                    throw new NuaParseException("Require body expressions after 'for' iterable expression");

                if (cursor < 0 || cursor >= tokens.Count ||
                    tokens[cursor].Kind != TokenKind.BigBracketRight)
                    throw new NuaParseException("Require bit right bracket after 'for body' expressions");
                cursor++;

                index = cursor;
                expr = new ForInExpr(valueNameExpr.Name, keyNameExpr?.Name, iterable, body);
                return true;
            }
            else
            {
                if (keyNameExpr != null)
                    throw new NuaParseException("Only one variable name is needed in 'for-of' expression");

                if (!ChainExpr.Match(tokens, ref cursor, out var chain))
                    throw new NuaParseException("Require interate values after 'of' keyword in 'for' expression");

                if (chain.Expressions.Count < 2 ||
                    chain.Expressions.Count > 3)
                    throw new NuaParseException("Invlaid interate value count after 'of' keyword in 'for' expression, must be 2 or 3");

                Expr startExpr = chain.Expressions[0];
                Expr endExpr = chain.Expressions[1];
                Expr? stepExpr = null;
                if (chain.Expressions.Count == 3)
                    stepExpr = chain.Expressions[2];

                if (cursor < 0 || cursor >= tokens.Count ||
                    tokens[cursor].Kind != TokenKind.BigBracketLeft)
                    throw new NuaParseException("Require big left bracket after 'for' iterable values");
                cursor++;

                if (!MultiExpr.Match(tokens, ref cursor, out var body))
                    throw new NuaParseException("Require body expressions after 'for' iterable values");

                if (cursor < 0 || cursor >= tokens.Count ||
                    tokens[cursor].Kind != TokenKind.BigBracketRight)
                    throw new NuaParseException("Require bit right bracket after 'for body' expressions");
                cursor++;

                index = cursor;
                expr = new ForOfExpr(valueNameExpr.Name, startExpr, endExpr, stepExpr, body);
                return true;
            }
        }
    }
}
