using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nua.CompileService.Syntaxes;
using Nua.Types;
using static Nua.CompileService.Parser;
using static Nua.CompileService.Syntaxes.Expr;

namespace Nua.CompileService;

public class Parser
{
    private readonly ParseStatus _status;
    private readonly IList<Token> _tokens;

    private readonly Matcher[] processExprMatchers;
    private readonly Matcher[] exprMatchers;
    private readonly Matcher[] valueExprMatchers;
    private readonly Matcher[] unaryExprMatchers;
    private readonly Matcher[] primaryExprMatchers;

    public IList<Token> Tokens => _tokens;
    public ParseStatus Status => _status;

    public delegate bool Matcher(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr);


    public Parser(IList<Token> tokens)
    {
        _tokens = tokens;


        processExprMatchers = new Matcher[]
        {
        MatchReturnExpr,
        MatchBreakExpr,
        MatchContinueExpr,
        MatchWhileExpr,
        MatchForExpr,
        MatchIfExpr,
        MatchGlobalExpr,
        MatchUnaryExpr,
        };

        exprMatchers = new Matcher[]
        {
        MatchAssignExpr,
        MatchOrExpr,
        };

        valueExprMatchers = new Matcher[]
        {
        MatchFuncExpr,
        MatchTableExpr,
        MatchListExpr,
        MatchQuotedExpr,
        MatchVariableExpr,
        MatchConstExpr,
        };

        unaryExprMatchers = new Matcher[]
        {
        MatchPrefixSelfAddExpr,
        MatchInvertNumberExpr,
        MatchPrimaryExpr,
        };

        primaryExprMatchers = new Matcher[]
        {
        MatchSuffixSelfAddExpr,
        MatchValueAccessExpr,
        MatchValueExpr,
        };
    }

    public Expr Parse()
    {
        int cursor = 0;
        if (!MatchExpr(true, ref cursor, out var parseStatus, out var expr))
        {
            if (parseStatus.Message == null)
                parseStatus.Message = "Invalid expression";

            throw new NuaParseException(parseStatus);
        }

        if (cursor < _tokens.Count)
            throw new NuaParseException(new ParseStatus(false, false, $"Unexpected token '{_tokens[cursor]}'"));

        return expr;
    }

    public MultiExpr ParseMulti()
    {
        int cursor = 0;
        if (!MatchMultiExpr(true, ref cursor, out var parseStatus, out var expr))
        {
            if (parseStatus.Message == null)
                parseStatus.Message = "Invalid expression";

            throw new NuaParseException(parseStatus);
        }
        if (cursor < _tokens.Count)
            throw new NuaParseException(new ParseStatus(false, false, $"Unexpected token '{_tokens[cursor]}'"));

        return expr;
    }


    public bool TokenMatch(bool required, TokenKind requiredTokenKind, ref int index, out bool requireToken, out Token token)
    {
        token = default;
        if (index < 0 || index >= _tokens.Count)
        {
            requireToken = required;
            return false;
        }

        if (_tokens[index].Kind != requiredTokenKind)
        {
            requireToken = false;
            return false;
        }

        token = _tokens[index];
        index++;
        requireToken = false;

        return true;
    }

    public bool MatchAddExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!MatchMulExpr(required, ref cursor, out parseStatus, out var left))
            return false;
        if (!MatchAddTailExpr(false, ref cursor, out var tailParseStatus, out var tail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = tail != null ? new AddExpr(left, tail) : left;
        return true;
    }

    public bool MatchAddTailExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out AddTailSyntax? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        Token operatorToken;
        if (!TokenMatch(required, TokenKind.OptAdd, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
            !TokenMatch(required, TokenKind.OptMin, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken))
        {
            parseStatus.Message = null;
            return false;
        }

        var operation = operatorToken.Kind switch
        {
            TokenKind.OptAdd => AddOperation.Add,
            TokenKind.OptMin => AddOperation.Min,
            _ => AddOperation.Add
        };

        if (!MatchMulExpr(true, ref cursor, out parseStatus, out var right))
        {
            parseStatus.Intercept = true;
            return false;
        }

        if (!MatchAddTailExpr(false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
        {
            parseStatus.Intercept = true;
            parseStatus.Message = tailParseStatus.Message;
            return false;
        }

        index = cursor;
        expr = new AddTailSyntax(right, operation, nextTail);
        parseStatus.RequireMoreTokens = false;
        parseStatus.Message = null;
        return true;
    }

    public bool MatchMulExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!MatchProcessExpr(required, ref cursor, out parseStatus, out var left))
            return false;
        if (!MatchMulTailExpr(false, ref cursor, out var tailParseStatus, out var tail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = tail != null ? new MulExpr(left, tail) : left;
        return true;
    }

    public bool MatchMulTailExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out MulTailSyntax? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        Token operatorToken;
        if (!TokenMatch(required, TokenKind.OptMul, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
            !TokenMatch(required, TokenKind.OptDiv, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
            !TokenMatch(required, TokenKind.OptPow, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
            !TokenMatch(required, TokenKind.OptMod, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
            !TokenMatch(required, TokenKind.OptDivInt, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken))
        {
            parseStatus.Message = null;
            return false;
        }

        var operation = operatorToken.Kind switch
        {
            TokenKind.OptMul => MulOperation.Mul,
            TokenKind.OptDiv => MulOperation.Div,
            TokenKind.OptPow => MulOperation.Pow,
            TokenKind.OptMod => MulOperation.Mod,
            TokenKind.OptDivInt => MulOperation.DivInt,
            _ => MulOperation.Mul
        };

        if (!MatchProcessExpr(true, ref cursor, out parseStatus, out var right))
        {
            parseStatus.Intercept = true;
            if (parseStatus.Message == null)
                parseStatus.Message = "Expect expression after '*','/','**','//','%' while parsing 'mul-expression'";

            return false;
        }

        if (!MatchMulTailExpr(false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = new MulTailSyntax(right, operation, nextTail);
        parseStatus.RequireMoreTokens = false;
        parseStatus.Message = null;
        return true;
    }

    public bool MatchProcessExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus.RequireMoreTokens = required;
        parseStatus.Message = null;
        parseStatus = new();
        expr = null;

        for (int i = 0; i < processExprMatchers.Length; i++)
        {
            var matcher = processExprMatchers[i];
            bool isLast = i == processExprMatchers.Length - 1;

            if (matcher.Invoke(isLast ? required : false, ref index, out parseStatus, out expr))
                return true;
            else if (parseStatus.Intercept)
                return false;
        }

        return false;
    }

    public bool MatchExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus.RequireMoreTokens = required;
        parseStatus.Message = null;
        parseStatus = new();
        expr = null;

        for (int i = 0; i < exprMatchers.Length; i++)
        {
            var matcher = exprMatchers[i];
            bool isLast = i == exprMatchers.Length - 1;

            if (matcher.Invoke(isLast ? required : false, ref index, out parseStatus, out expr))
                return true;
            else if (parseStatus.Intercept)
                return false;
        }

        return false;
    }

    public bool MatchReturnExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.KwdReturn, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Message = null;
            return false;
        }

        Expr? value = null;
        if (TokenMatch(false, TokenKind.OptColon, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            if (!MatchExpr(true, ref cursor, out parseStatus, out value))
            {
                if (parseStatus.Message == null)
                    parseStatus.Message = "Require expression after ':' while parsing 'return-expression'";

                return false;
            }
        }

        index = cursor;
        expr = new ReturnExpr(value);
        parseStatus.Message = null;
        return true;
    }

    public bool MatchBreakExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        parseStatus.Message = null;

        if (!TokenMatch(required, TokenKind.KwdBreak, ref index, out parseStatus.RequireMoreTokens, out _))
            return false;

        expr = new BreakExpr();
        return true;
    }

    public bool MatchContinueExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        parseStatus.Message = null;

        if (!TokenMatch(required, TokenKind.KwdContinue, ref index, out parseStatus.RequireMoreTokens, out _))
            return false;

        expr = new ContinueExpr();
        return true;
    }

    public bool MatchWhileExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.KwdWhile, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = required;
            return false;
        }

        if (!MatchExpr(true, ref cursor, out parseStatus, out var conditionExpr))
        {
            parseStatus.Intercept = true;

            if (parseStatus.Message == null)
                parseStatus.Message = "Require 'condition-expression' after 'while' keyword while parsing 'while-expression'";

            return false;
        }

        if (!TokenMatch(true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Require '{' after 'condition-expression' while parsing 'while-expression'";
            return false;
        }

        if (!MatchMultiExpr(false, ref cursor, out var bodyParseStatus, out var bodyExpr) && bodyParseStatus.Intercept)
        {
            parseStatus = bodyParseStatus;
            return false;
        }

        if (!TokenMatch(true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;

            if (bodyExpr != null)
                parseStatus.Message = "Require '}' after 'body-expression' while parsing 'while-expression'";
            else
                parseStatus.Message = "Require 'body-expression' after '{' while parsing 'while-expression'";

            return false;
        }

        index = cursor;
        expr = new WhileExpr(conditionExpr, bodyExpr);
        return true;
    }

    public bool MatchMultiExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out MultiExpr? expr)
    {
        parseStatus = new();
        expr = null;
        List<Expr> expressions = new();

        int cursor = index;
        while (MatchExpr(required, ref cursor, out parseStatus, out var oneExpr))
            expressions.Add(oneExpr);

        if (parseStatus.Intercept)
            return false;

        if (expressions.Count == 0)
            return false;

        index = cursor;
        expr = new MultiExpr(expressions);
        return true;
    }

    public bool MatchForExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.KwdFor, ref cursor, out _, out _))
        {
            parseStatus.Intercept = required;
            parseStatus.Message = null;
            return false;
        }

        VariableExpr valueNameExpr;
        if (!MatchVariableExpr(true, ref cursor, out parseStatus, out var _valueNameExpr))
        {
            parseStatus.Intercept = true;
            if (parseStatus.Message == null)
                parseStatus.Message = "Require variable after 'for' keyword";

            return false;
        }

        valueNameExpr = (VariableExpr)_valueNameExpr;

        if (cursor >= _tokens.Count)
        {
            parseStatus.Intercept = true;
            parseStatus.RequireMoreTokens = true;
            parseStatus.Message = "Require 'in' or 'of' after variable name in 'for' expression";
            return false;
        }

        VariableExpr? keyNameExpr = null;
        if (TokenMatch(false, TokenKind.OptComma, ref cursor, out _, out _))
        {
            if (!MatchVariableExpr(true, ref cursor, out parseStatus, out var _keyNameExpr))
            {
                parseStatus.Intercept = true;
                if (parseStatus.Message == null)
                    parseStatus.Message = "Require variable after comma in 'for' expression";

                return false;
            }

            keyNameExpr = (VariableExpr)_keyNameExpr;
            (keyNameExpr, valueNameExpr) = (valueNameExpr, keyNameExpr);

            if (cursor >= _tokens.Count)
            {
                parseStatus.Intercept = true;
                parseStatus.RequireMoreTokens = true;
                parseStatus.Message = "Require 'in' or 'of' after variable name in 'for' expression";
                return false;
            }
        }

        Token forKindToken;
        if (!TokenMatch(true, TokenKind.KwdIn, ref cursor, out parseStatus.RequireMoreTokens, out forKindToken) &&
            !TokenMatch(true, TokenKind.KwdOf, ref cursor, out parseStatus.RequireMoreTokens, out forKindToken))
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
            if (!MatchExpr(true, ref cursor, out parseStatus, out var iterable))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require iterable expression after 'in' keyword of 'for' expression";
                return false;
            }

            if (!TokenMatch(true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require big left bracket after 'for' iterable expression";
                return false;
            }

            if (!MatchMultiExpr(true, ref cursor, out parseStatus, out var body) && parseStatus.Intercept)
            {
                parseStatus.Message = "Require body expressions after 'for' iterable expression";
                return false;
            }

            if (!TokenMatch(true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
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

            if (!MatchChainExpr(true, ref cursor, out parseStatus, out var chain))
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

            if (!TokenMatch(true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = true;
                parseStatus.Message = "Require big left bracket after 'for' iterable values";
                return false;
            }

            if (!MatchMultiExpr(true, ref cursor, out parseStatus, out var body) && parseStatus.Intercept)
            {
                //parseStatus.Message = "Require body expressions after 'for' iterable values";
                return false;
            }

            if (!TokenMatch(true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
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

    public bool MatchChainExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ChainExpr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        List<Expr> expressions = new();

        if (!MatchExpr(required, ref cursor, out parseStatus, out var firstExpr))
            return false;

        expressions.Add(firstExpr);

        while (TokenMatch(false, TokenKind.OptComma, ref cursor, out _, out _))
        {
            if (!MatchExpr(true, ref cursor, out parseStatus, out var nextExpr))
            {
                if (parseStatus.Message == null)
                    parseStatus.Message = "Expect expression after ',' while parsing 'chain-expression'";

                return false;
            }

            expressions.Add(nextExpr);
        }

        index = cursor;
        expr = new ChainExpr(expressions);
        return true;
    }

    public bool MatchVariableExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        parseStatus.RequireMoreTokens = required;
        parseStatus.Message = null;


        if (!TokenMatch(required, TokenKind.Identifier, ref index, out _, out var idToken))
            return false;

        expr = new VariableExpr(idToken);
        return true;
    }

    public bool MatchValueExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus.RequireMoreTokens = required;
        parseStatus.Message = null;
        parseStatus = new();
        expr = null;

        for (int i = 0; i < valueExprMatchers.Length; i++)
        {
            var matcher = valueExprMatchers[i];
            bool isLast = i == valueExprMatchers.Length - 1;

            if (matcher.Invoke(isLast ? required : false, ref index, out parseStatus, out expr))
                return true;
            else if (parseStatus.Intercept)
                return false;
        }

        return false;
    }

    public bool MatchOrExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!MatchAndExpr(required, ref cursor, out parseStatus, out var left))
            return false;
        if (!MatchOrTailExpr(false, ref cursor, out var tailParseStatus, out var tail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = tail != null ? new OrExpr(left, tail) : left;
        return true;
    }

    public bool MatchAndExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!MatchEqualExpr(required, ref cursor, out parseStatus, out var left))
            return false;
        if (MatchAndTailExpr(false, ref cursor, out var tailParseStatus, out var tail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = tail != null ? new AndExpr(left, tail) : left;
        return true;
    }

    public bool MatchEqualExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!MatchCompareExpr(required, ref cursor, out parseStatus, out var left))
            return false;
        if (!MatchEqualTailExpr(false, ref cursor, out var tailParseStatus, out var tail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = tail != null ? new EqualExpr(left, tail) : left;
        return true;
    }

    public bool MatchCompareExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!MatchAddExpr(required, ref cursor, out parseStatus, out var left))
            return false;
        if (!MatchCompareTailExpr(false, ref cursor, out var tailParseStatus, out var tail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = tail != null ? new CompareExpr(left, tail) : left;
        return true;
    }

    public bool MatchCompareTailExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out CompareTailSyntax? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        Token operatorToken;
        if (!TokenMatch(required, TokenKind.OptLss, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
            !TokenMatch(required, TokenKind.OptGtr, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
            !TokenMatch(required, TokenKind.OptLeq, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
            !TokenMatch(required, TokenKind.OptGeq, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken))
        {
            parseStatus.Message = null;
            return false;
        }

        CompareOperation operation = operatorToken.Kind switch
        {
            TokenKind.OptLss => CompareOperation.LessThan,
            TokenKind.OptGtr => CompareOperation.GreaterThan,
            TokenKind.OptLeq => CompareOperation.LessEqual,
            TokenKind.OptGeq => CompareOperation.GreaterEqual,
            _ => CompareOperation.LessThan,
        };

        if (!MatchAddExpr(true, ref cursor, out parseStatus, out var right))
            return false;

        if (!MatchCompareTailExpr(false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = new CompareTailSyntax(right, operation, nextTail);
        parseStatus.RequireMoreTokens = false;
        parseStatus.Message = null;
        return true;
    }

    public bool MatchEqualTailExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out EqualTailSyntax? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        Token operatorToken;
        if (!TokenMatch(required, TokenKind.OptEql, ref cursor, out _, out operatorToken) &&
            !TokenMatch(required, TokenKind.OptNeq, ref cursor, out _, out operatorToken))
        {
            parseStatus.RequireMoreTokens = required;
            parseStatus.Message = null;
            return false;
        }

        EqualOperation operation = operatorToken.Kind switch
        {
            TokenKind.OptEql => EqualOperation.Equal,
            TokenKind.OptNeq => EqualOperation.NotEqual,
            _ => EqualOperation.Equal,
        };

        if (!MatchCompareExpr(true, ref cursor, out parseStatus, out var right))
        {
            parseStatus.Intercept = true;
            return false;
        }

        if (!MatchEqualTailExpr(false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = new EqualTailSyntax(right, operation, nextTail);
        parseStatus.RequireMoreTokens = false;
        parseStatus.Message = null;
        return true;
    }

    public bool MatchOrTailExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out OrTailSyntax? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.KwdOr, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = required;
            parseStatus.Message = null;
            return false;
        }

        if (!MatchAndExpr(true, ref cursor, out parseStatus, out var right))
        {
            parseStatus.Intercept = true;
            if (parseStatus.Message == null)
                parseStatus.Message = "Expect 'and-expression' after 'or' keyword";

            return false;
        }

        if (!MatchOrTailExpr(false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = new OrTailSyntax(right, nextTail);
        parseStatus.RequireMoreTokens = false;
        parseStatus.Message = null;
        return true;
    }

    public bool MatchAndTailExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out AndTailSyntax? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.KwdAnd, ref cursor, out _, out _))
        {
            parseStatus.RequireMoreTokens = required;
            parseStatus.Message = null;
            return false;
        }

        if (!MatchEqualExpr(true, ref cursor, out parseStatus, out var right))
        {
            if (parseStatus.Message == null)
                parseStatus.Message = "Require 'equal-expression' after 'and' keyword";

            return false;
        }

        if (!MatchAndTailExpr(false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
        {
            parseStatus.RequireMoreTokens = true;
            parseStatus.Message = tailParseStatus.Message;
            return false;
        }

        index = cursor;
        expr = new AndTailSyntax(right, nextTail);
        parseStatus.RequireMoreTokens = false;
        parseStatus.Message = null;
        return true;
    }

    public bool MatchIfExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.KwdIf, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = required;
            parseStatus.Message = null;
            return false;
        }

        if (!MatchExpr(true, ref cursor, out parseStatus, out var condition))
        {
            parseStatus.Intercept = true;
            if (parseStatus.Message == null)
                parseStatus.Message = "Require 'if' condition";

            return false;
        }

        if (!TokenMatch(true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Require '{' after 'if' condition";
            return false;
        }

        if (!MatchMultiExpr(false, ref cursor, out var bodyParseStatus, out var body) && bodyParseStatus.Intercept)
        {
            parseStatus = bodyParseStatus;
            return false;
        }

        if (!TokenMatch(true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Require '}' after 'if body' expressions";
            return false;
        }

        List<ElseIfExpr>? elseifs = null;

        while (MatchElseIfExpr(false, ref cursor, out parseStatus, out var elseif))
        {
            if (elseifs == null)
                elseifs = new();

            elseifs.Add(elseif);
        }

        if (parseStatus.Intercept)
            return false;

        if (!MatchElseExpr(false, ref cursor, out var elseParseStatus, out var elseExpr) && elseParseStatus.Intercept)
        {
            parseStatus = elseParseStatus;
            return false;
        }

        index = cursor;
        expr = new IfExpr(condition, body, elseifs, elseExpr);
        return true;
    }

    public bool MatchElseExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ElseSyntax? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.KwdElse, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = required;
            parseStatus.Message = null;
            return false;
        }

        if (!TokenMatch(true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Require '{' after 'else' keyword while parsing 'else-expression'";
            return false;
        }

        if (!MatchMultiExpr(false, ref cursor, out parseStatus, out var body) && parseStatus.Intercept)
            return false;

        if (!TokenMatch(true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;

            if (body != null)
                parseStatus.Message = "Require '}' after '{' while parsing 'else-expression'";
            else
                parseStatus.Message = "Require body expressions after '{' while parsing 'else-expression'";

            return false;
        }

        index = cursor;
        expr = new ElseSyntax(body);
        return true;
    }

    public bool MatchElseIfExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ElseIfExpr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.KwdElif, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = required;
            parseStatus.Message = null;
            return false;
        }

        if (!MatchExpr(true, ref cursor, out parseStatus, out var condition))
        {
            parseStatus.Intercept = true;
            if (parseStatus.Message == null)
                parseStatus.Message = "Require 'elif' condition";

            return false;
        }

        if (!TokenMatch(true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Require big left bracket after 'elif' condition";
            return false;
        }

        if (!MatchMultiExpr(false, ref cursor, out parseStatus, out var body) && parseStatus.Intercept)
            return false;

        if (!TokenMatch(true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Require big right bracket after 'elif' condition";
            return false;
        }

        index = cursor;
        expr = new ElseIfExpr(condition, body);
        return true;
    }

    public bool MatchAssignExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!MatchOrExpr(required, ref cursor, out parseStatus, out var left))
            return false;
        if (!MatchAssignTailExpr(false, ref cursor, out parseStatus, out var tail) && parseStatus.Intercept)
            return false;

        index = cursor;
        expr = tail != null ? new AssignExpr(left, tail) : left;
        return true;
    }

    public bool MatchAssignTailExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out AssignTailSyntax? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        Token operatorToken;
        if (!TokenMatch(required, TokenKind.OptAssign, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
            !TokenMatch(required, TokenKind.OptAddWith, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
            !TokenMatch(required, TokenKind.OptMinWith, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken))
        {
            parseStatus.Intercept = required;
            parseStatus.Message = null;
            return false;
        }

        AssignOperation operation = operatorToken.Kind switch
        {
            TokenKind.OptAssign => AssignOperation.Assign,
            TokenKind.OptAddWith => AssignOperation.AddWith,
            TokenKind.OptMinWith => AssignOperation.MinWith,
            _ => default
        };

        if (!MatchOrExpr(required, ref cursor, out parseStatus, out var right))
        {
            if (parseStatus.Message == null)
                parseStatus.Message = "Require expression after '=' token while parsing 'assign-tail-expression'";

            return false;
        }

        if (!MatchAssignTailExpr(false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = new AssignTailSyntax(right, operation, nextTail);
        parseStatus.RequireMoreTokens = false;
        parseStatus.Message = null;
        return true;
    }

    public bool MatchGlobalExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.KwdGlobal, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = required;
            return false;
        }

        if (!TokenMatch(true, TokenKind.Identifier, ref cursor, out parseStatus.RequireMoreTokens, out var variableNameToken))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Require identifier after 'global' keyword while parsing 'global-expression";
            return false;
        }

        index = cursor;
        expr = new GlobalExpr(variableNameToken.Value!);
        return true;
    }

    public bool MatchUnaryExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus.RequireMoreTokens = required;
        parseStatus.Message = null;
        parseStatus = new();
        expr = null;

        for (int i = 0; i < unaryExprMatchers.Length; i++)
        {
            var matcher = unaryExprMatchers[i];
            bool isLast = i == unaryExprMatchers.Length - 1;

            if (matcher.Invoke(isLast ? required : false, ref index, out parseStatus, out expr))
                return true;
            else if (parseStatus.Intercept)
                return false;
        }

        return false;
    }

    public bool MatchFuncExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;
        if (!TokenMatch(required, TokenKind.KwdFunction, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = required;
            parseStatus.Message = null;
            return false;
        }

        if (!TokenMatch(true, TokenKind.ParenthesesLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Require '(' after 'function' keyword while parsing function";
            return false;
        }

        List<Token> parameterNameTokens = new();
        if (TokenMatch(false, TokenKind.Identifier, ref cursor, out _, out var firstParameterName))
        {
            parameterNameTokens.Add(firstParameterName);

            while (TokenMatch(false, TokenKind.OptComma, ref cursor, out _, out _))
            {
                if (!TokenMatch(true, TokenKind.Identifier, ref cursor, out parseStatus.RequireMoreTokens, out var anotherParameterName))
                {
                    parseStatus.Intercept = true;
                    parseStatus.Message = "Require parameter name after ',' token while parsing function";
                    return false;
                }

                parameterNameTokens.Add(anotherParameterName);
            }
        }

        if (!TokenMatch(true, TokenKind.ParenthesesRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            if (parameterNameTokens.Count != 0)
                parseStatus.Message = "Require ')' after parameters while parsing function";
            else
                parseStatus.Message = "Require parameters after '(' token while parsing function";

            return false;
        }

        if (!TokenMatch(true, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Require '{' after ')' while parsing function";
            return false;
        }

        if (!MatchMultiExpr(false, ref cursor, out var bodyParseStatus, out var body) && bodyParseStatus.Intercept)
        {
            parseStatus = bodyParseStatus;
            return false;
        }

        if (!TokenMatch(true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Require '}' after function body while parsing function";
            return false;
        }

        index = cursor;
        expr = new FuncExpr(parameterNameTokens, body);
        parseStatus.Message = null;
        return true;
    }

    public bool MatchTableExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.BigBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Message = null;
            return false;
        }

        List<TableMemberSyntax> members = new();
        while (MatchTableMemberSyntax(false, ref cursor, out parseStatus, out var member))
        {
            members.Add(member);

            if (!TokenMatch(false, TokenKind.OptComma, ref cursor, out _, out _))
                break;
        }

        if (parseStatus.Intercept)
            return false;

        if (!TokenMatch(true, TokenKind.BigBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Expect '}' after '{' while parsing 'table-expression'";
            return false;
        }

        index = cursor;
        expr = new TableExpr(members);
        parseStatus.Message = null;
        return true;
    }

    public bool MatchTableMemberSyntax(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out TableMemberSyntax? syntax)
    {
        parseStatus = new();
        syntax = null;
        int cursor = index;

        Expr key;
        if (TokenMatch(required, TokenKind.Identifier, ref cursor, out parseStatus.RequireMoreTokens, out var idToken))
            key = new ConstExpr(new NuaString(idToken.Value!));
        else if (MatchConstExpr(required, ref cursor, out parseStatus, out var constKey))
            key = constKey;
        else
            return false;

        if (!TokenMatch(true, TokenKind.OptColon, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Expect ':' after table member name while parsing 'table-expression'";
            return false;
        }

        if (!MatchExpr(true, ref cursor, out parseStatus, out var value))
        {
            if (parseStatus.Message == null)
                parseStatus.Message = "Expect expression after ':' while parsing 'table-expression'";

            return false;
        }

        index = cursor;
        syntax = new TableMemberSyntax(key, value);
        return true;
    }

    public bool MatchConstExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        if (index < 0 || index >= _tokens.Count)
        {
            parseStatus.RequireMoreTokens = required;
            parseStatus.Message = null;
            return false;
        }

        var token = _tokens[index];

        if (token.Kind == TokenKind.Number && token.Value != null)
            expr = new ConstExpr(new NuaNumber(double.Parse(token.Value)));
        else if (token.Kind == TokenKind.String && token.Value != null)
            expr = new ConstExpr(new NuaString(token.Value));
        else if (token.Kind == TokenKind.KwdTrue)
            expr = new ConstExpr(new NuaBoolean(true));
        else if (token.Kind == TokenKind.KwdFalse)
            expr = new ConstExpr(new NuaBoolean(false));
        else if (token.Kind == TokenKind.KwdNull)
            expr = new ConstExpr(null);

        if (expr != null)
            index++;

        parseStatus.RequireMoreTokens = false;
        parseStatus.Message = null;
        return expr != null;
    }

    public bool MatchPrefixSelfAddExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        Token operatorToken;
        if (!TokenMatch(required, TokenKind.OptDoubleAdd, ref cursor, out _, out operatorToken) &&
            !TokenMatch(required, TokenKind.OptDoubleMin, ref cursor, out _, out operatorToken))
        {
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return false;
        }

        bool negative = operatorToken.Kind == TokenKind.OptDoubleMin;

        if (!MatchValueAccessExpr(true, ref cursor, out parseStatus, out var self))
        {
            if (parseStatus.Message == null)
                parseStatus.Message = "Expect 'value-access-expressoin' or 'variable-expression' after '++' or '--' while parsing 'prefix-self-add-expression'";

            return false;
        }

        index = cursor;
        expr = new PrefixSelfAddExpr(self, negative);
        return true;
    }

    public bool MatchValueAccessExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!MatchValueExpr(required, ref cursor, out parseStatus, out var variable))
            return false;

        if (!MatchValueAccessTailExpr(false, ref cursor, out var tailParseStatus, out var tail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        expr = tail != null ? new ValueAccessExpr((ValueExpr)variable, tail) : (ValueExpr)variable;
        index = cursor;
        return true;
    }

    public bool MatchValueAccessTailExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ValueAccessTailSyntax? expr)
    {
        parseStatus = new();
        expr = null;
        if (MatchValueIndexAccessTailExpr(required, ref index, out parseStatus, out var expr3))
            expr = expr3;
        else if (MatchValueInvokeAccessTailExpr(required, ref index, out parseStatus, out var expr2))
            expr = expr2;
        else if (MatchValueMemberAccessTailExpr(required, ref index, out parseStatus, out var expr1))
            expr = expr1;
        else
            return false;

        return true;
    }

    public bool MatchPrimaryExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus.RequireMoreTokens = required;
        parseStatus.Message = null;
        parseStatus = new();
        expr = null;

        for (int i = 0; i < primaryExprMatchers.Length; i++)
        {
            var matcher = primaryExprMatchers[i];
            bool isLast = i == primaryExprMatchers.Length - 1;

            if (matcher.Invoke(isLast ? required : false, ref index, out parseStatus, out expr))
                return true;
            else if (parseStatus.Intercept)
                return false;
        }

        return false;
    }

    public bool MatchSuffixSelfAddExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!MatchValueAccessExpr(required, ref cursor, out _, out var self))
        {
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return false;
        }


        Token operatorToken;
        if (!TokenMatch(false, TokenKind.OptDoubleAdd, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken) &&
            !TokenMatch(false, TokenKind.OptDoubleMin, ref cursor, out parseStatus.RequireMoreTokens, out operatorToken))
        {
            index = cursor;
            expr = self;
            parseStatus.Message = null;
            return true;
        }

        bool negative = operatorToken.Kind == TokenKind.OptDoubleMin;

        index = cursor;
        expr = new SuffixSelfAddExpr(self, negative);
        parseStatus.Message = null;
        return true;
    }

    public bool MatchValueIndexAccessTailExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ValueIndexAccessTailSyntax? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.SquareBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Message = null;
            return false;
        }

        if (!MatchExpr(required, ref cursor, out parseStatus, out var indexExpr))
        {
            parseStatus.Intercept = true;
            if (parseStatus.Message == null)
                parseStatus.Message = "Require index after '[' while parsing 'value-access-expression'";

            return false;
        }

        if (!TokenMatch(required, TokenKind.SquareBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Message = "Require ']' after index while parsing 'value-access-expression'";
            return false;
        }

        if (!MatchValueAccessTailExpr(false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = new ValueIndexAccessTailSyntax(indexExpr, nextTail);
        return true;
    }

    public bool MatchListExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.SquareBracketLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Message = null;
            return false;
        }

        List<Expr> members = new();
        while (MatchExpr(false, ref cursor, out parseStatus, out var member))
        {
            members.Add(member);

            if (!TokenMatch(false, TokenKind.OptComma, ref cursor, out _, out _))
                break;
        }

        if (parseStatus.Intercept)
            return false;

        if (!TokenMatch(true, TokenKind.SquareBracketRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;

            if (members.Count == 0)
                parseStatus.Message = "Expect ']' after list member while parsing 'list-expression'";
            else
                parseStatus.Message = "Expect list member after '[' while parsing 'list-expression'";

            return false;
        }

        index = cursor;
        expr = new ListExpr(members);
        parseStatus.Message = null;
        return true;
    }

    public bool MatchInvertNumberExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.OptMin, ref cursor, out _, out _))
        {
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return false;
        }

        if (!MatchPrimaryExpr(true, ref cursor, out parseStatus, out var toInvert))
        {
            if (parseStatus.Message == null)
                parseStatus.Message = "Expect 'primary-expression' after '-' while parsing 'invert-number-expression'";

            return false;
        }

        index = cursor;
        expr = new InvertNumberExpr(toInvert);
        return true;
    }

    public bool MatchValueInvokeAccessTailExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ValueInvokeAccessTailSyntax? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        // 匹配左括号
        if (!TokenMatch(required, TokenKind.ParenthesesLeft, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Message = null;
            return false;
        }

        List<Expr> positionParams = new();
        List<KeyValuePair<string, Expr>> namedParams = new();
        if (MatchValueInvokeParameterSyntax(false, ref cursor, out parseStatus, out var firstParam))
        {
            if (firstParam.Name is not null)
                namedParams.Add(new KeyValuePair<string, Expr>(firstParam.Name, firstParam.ValueExpr));
            else
                positionParams.Add(firstParam.ValueExpr);

            while (TokenMatch(false, TokenKind.OptComma, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                if (!MatchValueInvokeParameterSyntax(true, ref cursor, out parseStatus, out var otherParam))
                    return false;

                if (otherParam.Name is not null)
                {
                    namedParams.Add(new KeyValuePair<string, Expr>(otherParam.Name, otherParam.ValueExpr));
                }
                else
                {
                    if (namedParams.Count != 0)
                    {
                        parseStatus.Intercept = true;
                        parseStatus.Message = "The named parameter must be after the positional parameter while parsing 'value-invoke-acess-tail-expression'";
                        return false;
                    }

                    positionParams.Add(otherParam.ValueExpr);
                }
            }
        }

        if (parseStatus.Intercept)
            return false;

        if (!TokenMatch(true, TokenKind.ParenthesesRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Intercept = true;
            parseStatus.Message = "Require parameters or ')' after '(' while parsing 'value-access-expression'";
            return false;
        }

        if (!MatchValueAccessTailExpr(false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        index = cursor;
        expr = new ValueInvokeAccessTailSyntax(positionParams, namedParams, nextTail);
        parseStatus.Message = null;
        return true;
    }

    public bool MatchValueInvokeParameterSyntax(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ValueInvokeParameterSyntax? syntax)
    {
        syntax = null;
        parseStatus = new();
        int cursor = index;

        bool hasName =
            TokenMatch(false, TokenKind.Identifier, ref cursor, out _, out var nameToken) &&
            TokenMatch(false, TokenKind.OptColon, ref cursor, out _, out _);

        if (!hasName)
            cursor = index;

        if (!MatchExpr(required, ref cursor, out parseStatus, out var valueExpr))
            return false;

        index = cursor;
        syntax = new ValueInvokeParameterSyntax(valueExpr, hasName ? nameToken.Value! : null);
        return true;
    }

    public bool MatchValueMemberAccessTailExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ValueMemberAccessTailSyntax? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.OptDot, ref cursor, out _, out _))
        {
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return false;
        }

        if (!TokenMatch(true, TokenKind.Identifier, ref cursor, out parseStatus.RequireMoreTokens, out var idToken))
        {
            parseStatus.Message = "Require identifier after '.' while parsing 'value-access-expression'";
            return false;
        }

        if (!MatchValueAccessTailExpr(false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
        {
            parseStatus = tailParseStatus;
            return false;
        }

        expr = new ValueMemberAccessTailSyntax(idToken, nextTail);
        index = cursor;
        parseStatus.Message = null;
        return true;
    }

    public bool MatchQuotedExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.ParenthesesLeft, ref cursor, out _, out _))
        {
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return false;
        }

        if (!MatchExpr(required, ref cursor, out parseStatus, out var content))
        {
            if (parseStatus.Message == null)
                parseStatus.Message = ("Expect expression after '(' token while parsing 'quote-expressoin'");

            return false;
        }

        if (!TokenMatch(required, TokenKind.ParenthesesRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Message = "Expect ')' after expression while parsing 'quote-expressoin'";
            return false;
        }

        index = cursor;
        expr = new QuotedExpr(content);
        return true;
    }

    public bool MatchQuotedChainExpr(bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out QuotedChainExpr? expr)
    {
        parseStatus = new();
        expr = null;
        int cursor = index;

        if (!TokenMatch(required, TokenKind.ParenthesesLeft, ref cursor, out _, out _))
        {
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return false;
        }

        if (!MatchChainExpr(true, ref cursor, out parseStatus, out var chain))
            return false;

        if (!TokenMatch(true, TokenKind.ParenthesesRight, ref cursor, out parseStatus.RequireMoreTokens, out _))
        {
            parseStatus.Message = "Require ')' after 'chain-expression' while parsing 'quoted-chain-expression'";
            return false;
        }

        index = cursor;
        expr = new QuotedChainExpr(chain);
        return true;
    }

}
