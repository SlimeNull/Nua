using System.Diagnostics.CodeAnalysis;

namespace Nua.CompileService.Syntaxes
{
    public abstract class ForExpr : ProcessExpr
    {
        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdFor, ref cursor, out _, out _))
            {
                parseStatus.Intercept = required;
                parseStatus.Message = null;
                return false;
            }

            VariableExpr valueNameExpr;
            if (!VariableExpr.Match(tokens, true, ref cursor, out parseStatus, out var _valueNameExpr))
            {
                parseStatus.Intercept = true;
                if (parseStatus.Message == null)
                    parseStatus.Message = "Require variable after 'for' keyword";

                return false;
            }

            valueNameExpr = (VariableExpr)_valueNameExpr;

            if (cursor >= tokens.Count)
            {
                parseStatus.Intercept = true;
                parseStatus.RequireMoreTokens = true;
                parseStatus.Message = "Require 'in' or 'of' after variable name in 'for' expression";
                return false;
            }

            VariableExpr? keyNameExpr = null;
            if (TokenMatch(tokens, false, TokenKind.OptComma, ref cursor, out _, out _))
            {
                if (!VariableExpr.Match(tokens, true, ref cursor, out parseStatus, out var _keyNameExpr))
                {
                    parseStatus.Intercept = true;
                    if (parseStatus.Message == null)
                        parseStatus.Message = "Require variable after comma in 'for' expression";

                    return false;
                }

                keyNameExpr = (VariableExpr)_keyNameExpr;
                (keyNameExpr, valueNameExpr) = (valueNameExpr, keyNameExpr);

                if (cursor >= tokens.Count)
                {
                    parseStatus.Intercept = true;
                    parseStatus.RequireMoreTokens = true;
                    parseStatus.Message = "Require 'in' or 'of' after variable name in 'for' expression";
                    return false;
                }
            }

            Token forKindToken;
            if (!TokenMatch(tokens, true, TokenKind.KwdIn, ref cursor, out parseStatus.RequireMoreTokens, out forKindToken) &&
                !TokenMatch(tokens, true, TokenKind.KwdOf, ref cursor, out parseStatus.RequireMoreTokens, out forKindToken))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require 'in' or 'of' after variable name in 'for' expression";
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
                if (!Expr.Match(tokens, true, ref cursor, out parseStatus, out var iterable))
                {
                    parseStatus.Intercept = true;
                    parseStatus.Message = "Require iterable expression after 'in' keyword of 'for' expression";
                    return false;
                }

                if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
                {
                    parseStatus.Intercept = true;
                    parseStatus.Message = "Require big left bracket after 'for' iterable expression";
                    return false;
                }

                if (!MultiExpr.Match(tokens, true, ref cursor, out parseStatus, out var body) && parseStatus.Intercept)
                {
                    parseStatus.Message = "Require body expressions after 'for' iterable expression";
                    return false;
                }

                if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
                {
                    parseStatus.Intercept = true;
                    parseStatus.Message = "Require bit right bracket after 'for body' expressions";
                    return false;
                }

                index = cursor;
                expr = new ForInExpr(valueNameExpr.Name, keyNameExpr?.Name, iterable, body);
                parseStatus.Message = null;
                return true;
            }
            else
            {
                if (keyNameExpr != null)
                {
                    parseStatus.Intercept = true;
                    parseStatus.RequireMoreTokens = false;
                    parseStatus.Message = "Only one variable name is needed in 'for-of' expression";
                    return false;
                }

                if (!ChainExpr.Match(tokens, true, ref cursor, out parseStatus, out var chain))
                {
                    parseStatus.Intercept = true;
                    if (parseStatus.Message == null)
                        parseStatus.Message = "Require interate values after 'of' keyword in 'for' expression";

                    return false;
                }

                if (chain.Expressions.Count < 2 ||
                    chain.Expressions.Count > 3)
                {
                    parseStatus.Intercept = true;
                    parseStatus.RequireMoreTokens = false;
                    parseStatus.Message = "Invlaid interate value count after 'of' keyword in 'for' expression, must be 2 or 3";
                    return false;
                }

                Expr startExpr = chain.Expressions[0];
                Expr endExpr = chain.Expressions[1];
                Expr? stepExpr = null;
                if (chain.Expressions.Count == 3)
                    stepExpr = chain.Expressions[2];

                if (!TokenMatch(tokens, true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
                {
                    parseStatus.Intercept = true;
                    parseStatus.Message = "Require big left bracket after 'for' iterable values";
                    return false;
                }

                if (!MultiExpr.Match(tokens, true, ref cursor, out parseStatus, out var body) && parseStatus.Intercept)
                {
                    //parseStatus.Message = "Require body expressions after 'for' iterable values";
                    return false;
                }

                if (!TokenMatch(tokens, true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
                {
                    parseStatus.Intercept = true;
                    parseStatus.Message = "Require bit right bracket after 'for body' expressions";
                    return false;
                }

                index = cursor;
                expr = new ForOfExpr(valueNameExpr.Name, startExpr, endExpr, stepExpr, body);
                parseStatus.Message = null;
                return true;
            }
        }
    }
}
