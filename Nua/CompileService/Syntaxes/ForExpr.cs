using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes
{
    public abstract class ForExpr : ProcessExpr
    {
        public new static bool Match(IList<Token> tokens, bool required, ref int index, out bool requireMoreTokens, out string? message, [NotNullWhen(true)] out Expr? expr)
        {
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdFor, ref cursor, out _, out _))
            {
                requireMoreTokens = false;
                message = null;
                return false;
            }

            VariableExpr valueNameExpr;
            if (!VariableExpr.Match(tokens, true, ref cursor, out requireMoreTokens, out message, out var _valueNameExpr))
            {
                if (message == null)
                    message = "Require variable after 'for' keyword";

                return false;
            }

            valueNameExpr = (VariableExpr)_valueNameExpr;

            if (cursor >= tokens.Count)
            {
                requireMoreTokens = true;
                message = "Require 'in' or 'of' after variable name in 'for' expression";
                return false;
            }

            VariableExpr? keyNameExpr = null;
            if (TokenMatch(tokens, false, TokenKind.OptComma, ref cursor, out _, out _))
            {
                if (!VariableExpr.Match(tokens, true, ref cursor, out requireMoreTokens, out message, out var _keyNameExpr))
                {
                    if (message == null)
                        message = "Require variable after comma in 'for' expression";

                    return false;
                }

                keyNameExpr = (VariableExpr)_keyNameExpr;
                (keyNameExpr, valueNameExpr) = (valueNameExpr, keyNameExpr);

                if (cursor >= tokens.Count)
                {
                    requireMoreTokens = true;
                    message = "Require 'in' or 'of' after variable name in 'for' expression";
                    return false;
                }
            }

            Token forKindToken;
            if (!TokenMatch(tokens, true, TokenKind.KwdIn, ref cursor, out requireMoreTokens, out forKindToken) &&
                !TokenMatch(tokens, true, TokenKind.KwdOf, ref cursor, out requireMoreTokens, out forKindToken))
            {
                message = "Require 'in' or 'of' after variable name in 'for' expression";
                return false;
            }

            ForOperation operation = forKindToken.Kind switch
            {
                TokenKind.KwdIn => ForOperation.In,
                TokenKind.KwdOf => ForOperation.Of,
                _ => default
            };

            if (operation == ForOperation.In)
            {
                if (!Expr.MatchAny(tokens, true, ref cursor, out requireMoreTokens, out _, out var iterable))
                {
                    message = "Require iterable expression after 'in' keyword of 'for' expression";
                    return false;
                }

                if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out requireMoreTokens, out _))
                {
                    message = "Require big left bracket after 'for' iterable expression";
                    return false;
                }

                if (!MultiExpr.Match(tokens, true, ref cursor, out requireMoreTokens, out _, out var body))
                {
                    message = "Require body expressions after 'for' iterable expression";
                    return false;
                }

                if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out requireMoreTokens, out _))
                {
                    message = "Require bit right bracket after 'for body' expressions";
                    return false;
                }

                index = cursor;
                expr = new ForInExpr(valueNameExpr.Name, keyNameExpr?.Name, iterable, body);
                message = null;
                return true;
            }
            else
            {
                if (keyNameExpr != null)
                {
                    requireMoreTokens = false;
                    message = "Only one variable name is needed in 'for-of' expression";
                    return false;
                }

                if (!ChainExpr.Match(tokens, true, ref cursor, out requireMoreTokens, out message, out var chain))
                {
                    if (message == null)
                        message = "Require interate values after 'of' keyword in 'for' expression";

                    return false;
                }

                if (chain.Expressions.Count < 2 ||
                    chain.Expressions.Count > 3)
                {
                    requireMoreTokens = false;
                    message = "Invlaid interate value count after 'of' keyword in 'for' expression, must be 2 or 3";
                    return false;
                }

                Expr startExpr = chain.Expressions[0];
                Expr endExpr = chain.Expressions[1];
                Expr? stepExpr = null;
                if (chain.Expressions.Count == 3)
                    stepExpr = chain.Expressions[2];

                if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out requireMoreTokens, out _))
                {
                    message = "Require big left bracket after 'for' iterable values";
                    return false;
                }

                if (!MultiExpr.Match(tokens, true, ref cursor, out requireMoreTokens, out _, out var body))
                {
                    message = "Require body expressions after 'for' iterable values";
                    return false;
                }

                if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out requireMoreTokens, out _))
                {
                    message = "Require bit right bracket after 'for body' expressions";
                    return false;
                }

                index = cursor;
                expr = new ForOfExpr(valueNameExpr.Name, startExpr, endExpr, stepExpr, body);
                message = null;
                return true;
            }
        }
    }
}
