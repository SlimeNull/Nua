using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nua.CompileService
{

    public class Lexer
    {
        public static IEnumerable<Token> Lex(TextReader reader)
        {
            int ln = 0;
            int col = 0;

            while (true)
            {
                col++;
                var ch = reader.Read();
                char cch = (char)ch;

                if (ch == -1)
                    yield break;

                if (char.IsWhiteSpace(cch))
                {
                    if (cch == '\r')
                    {
                        var next = reader.Peek();
                        if (next == '\n')
                            reader.Read();

                        col = 0;
                        ln++;
                    }
                    else if (ch == '\n')
                    {
                        col = 0;
                        ln++;
                    }


                    continue;
                }
                else if (char.IsLetter(cch) || cch == '_')
                {
                    int tokenLn = ln;
                    int tokenCol = col - 1;

                    StringBuilder sb = new StringBuilder();
                    sb.Append(cch);

                    while (true)
                    {
                        ch = reader.Peek();
                        cch = (char)ch;

                        if (char.IsLetterOrDigit(cch) || cch == '_')
                        {
                            col++;
                            reader.Read();
                            sb.Append(cch);
                        }
                        else
                        {
                            break;
                        }
                    }

                    TokenKind? kwdToken = sb.ToString() switch
                    {
                        "require" => TokenKind.KwdRequire,

                        "if" => TokenKind.KwdIf,
                        "else" => TokenKind.KwdElse,
                        "elif" => TokenKind.KwdElif,

                        "for" => TokenKind.KwdFor,
                        "in" => TokenKind.KwdIn,
                        "of" => TokenKind.KwdOf,
                        "loop" => TokenKind.KwdLoop,
                        "while" => TokenKind.KwdWhile,
                        "continue" => TokenKind.KwdContinue,
                        "break" => TokenKind.KwdBreak,

                        "null" => TokenKind.KwdNull,
                        "true" => TokenKind.KwdTrue,
                        "false" => TokenKind.KwdFalse,

                        "not" => TokenKind.KwdNot,
                        "and" => TokenKind.KwdAnd,
                        "or" => TokenKind.KwdOr,

                        "func" => TokenKind.KwdFunction,
                        "return" => TokenKind.KwdReturn,

                        "global" => TokenKind.KwdGlobal,

                        _ => null
                    };

                    if (kwdToken is TokenKind kwd)
                        yield return new Token(kwd, null, tokenLn, tokenCol);
                    else
                        yield return new Token(TokenKind.Identifier, sb.ToString(), tokenLn, tokenCol);

                    continue;
                }
                else if (char.IsDigit(cch))
                {
                    int tokenLn = ln;
                    int tokenCol = col - 1;

                    StringBuilder sb = new StringBuilder();
                    sb.Append(cch);

                    while (ch != -1)
                    {
                        ch = reader.Peek();
                        cch = (char)ch;
                        if (ch >= '0' && ch <= '9')
                        {
                            col++;
                            reader.Read();
                            sb.Append(cch);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (cch == '.')
                    {
                        col++;
                        ch = reader.Read();  // skip '.'
                        sb.Append(cch);
                        while (ch != -1)
                        {
                            ch = reader.Peek();
                            cch = (char)ch;
                            if (ch >= '0' && ch <= '9')
                            {
                                col++;
                                reader.Read();
                                sb.Append(cch);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (cch == 'e' || cch == 'E')
                    {
                        col++;
                        reader.Read();  // skip 'e'
                        sb.Append(cch);

                        col++;
                        ch = reader.Read();
                        if (ch >= '0' || ch <= '9' || ch == '+' || ch == '-')
                        {
                            col++;
                            reader.Read();
                            sb.Append((char)ch);
                        }

                        while (ch != -1)
                        {
                            ch = reader.Peek();
                            cch = (char)ch;
                            if (ch >= '0' && ch <= '9')
                            {
                                col++;
                                reader.Read();
                                sb.Append(cch);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    yield return new Token(TokenKind.Number, sb.ToString(), tokenLn, tokenCol);
                    continue;
                }
                else if (cch == '"')
                {
                    int tokenLn = ln;
                    int tokenCol = col - 1;

                    StringBuilder sb = new();

                    while (true)
                    {
                        col++;
                        var nextCh = reader.Read();

                        if (nextCh is -1 or '\r' or '\n')
                            throw new NuaLexException("String not closed");

                        var nextCch = (char)nextCh;
                        if (nextCch == '\\')
                        {
                            col++;
                            var escapeSeq = reader.Read();

                            if (escapeSeq == -1)
                                throw new NuaLexException("String not closed");

                            sb.Append(escapeSeq switch
                            {
                                't' => '\t',
                                'r' => '\r',
                                'n' => '\n',
                                'b' => '\b',
                                _ => throw new NuaLexException("Invalid escape sequence")
                            });
                        }
                        else if (nextCch == '"')
                        {
                            break;
                        }
                        else
                        {
                            sb.Append(nextCch);
                        }
                    }

                    yield return new Token(TokenKind.String, sb.ToString(), tokenLn, tokenCol);
                }
                else if (cch == '#')
                {
                    while (true)
                    {
                        var nextCh = reader.Peek();

                        if (nextCh is '\r' or '\n' or -1)
                            break;

                        col++;
                        reader.Read();
                    }
                }
                else
                {
                    int tokenLn = ln;
                    int tokenCol = col - 1;

                    switch (ch)
                    {
                        case '+':
                            if (reader.Peek() == '+')
                            {
                                col++;
                                reader.Read();
                                yield return new Token(TokenKind.OptDoubleAdd, null, tokenLn, tokenCol);
                            }
                            else if (reader.Peek() == '=')
                            {
                                col++;
                                reader.Read();
                                yield return new Token(TokenKind.OptAddWith, null, tokenLn, tokenCol);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptAdd, null, tokenLn, tokenCol);
                            }
                            break;
                        case '-':
                            if (reader.Peek() == '-')
                            {
                                col++;
                                reader.Read();
                                yield return new Token(TokenKind.OptDoubleMin, null, tokenLn, tokenCol);
                            }
                            else if (reader.Peek() == '=')
                            {
                                col++;
                                reader.Read();
                                yield return new Token(TokenKind.OptMinWith, null, tokenLn, tokenCol);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptMin, null, tokenLn, tokenCol);
                            }
                            break;
                        case '*':
                            if (reader.Peek() == '*')
                            {
                                col++;
                                reader.Read();
                                yield return new Token(TokenKind.OptPow, null, tokenLn, tokenCol);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptMul, null, tokenLn, tokenCol);
                            }
                            break;
                        case '/':
                            if (reader.Peek() == '/')
                            {
                                col++;
                                reader.Read();
                                yield return new Token(TokenKind.OptDivInt, null, tokenLn, tokenCol);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptDiv, null, tokenLn, tokenCol);
                            }
                            break;
                        case '%':
                            yield return new Token(TokenKind.OptMod, null, tokenLn, tokenCol);
                            break;
                        case '(':
                            yield return new Token(TokenKind.ParenthesesLeft, null, tokenLn, tokenCol);
                            break;
                        case ')':
                            yield return new Token(TokenKind.ParenthesesRight, null, tokenLn, tokenCol);
                            break;
                        case '[':
                            yield return new Token(TokenKind.SquareBracketLeft, null, tokenLn, tokenCol);
                            break;
                        case ']':
                            yield return new Token(TokenKind.SquareBracketRight, null, tokenLn, tokenCol);
                            break;
                        case '{':
                            yield return new Token(TokenKind.BigBracketLeft, null, tokenLn, tokenCol);
                            break;
                        case '}':
                            yield return new Token(TokenKind.BigBracketRight, null, tokenLn, tokenCol);
                            break;
                        case ':':
                            yield return new Token(TokenKind.OptColon, null, tokenLn, tokenCol);
                            break;
                        case ',':
                            yield return new Token(TokenKind.OptComma, null, tokenLn, tokenCol);
                            break;
                        case '.':
                            yield return new Token(TokenKind.OptDot, null, tokenLn, tokenCol);
                            break;
                        case '>':
                            if (reader.Peek() == '=')
                            {
                                col++;
                                reader.Read();
                                yield return new Token(TokenKind.OptGeq, null, tokenLn, tokenCol);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptGtr, null, tokenLn, tokenCol);
                            }
                            break;
                        case '<':
                            if (reader.Peek() == '=')
                            {
                                col++;
                                reader.Read();
                                yield return new Token(TokenKind.OptLeq, null, tokenLn, tokenCol);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptLss, null, tokenLn, tokenCol);
                            }
                            break;
                        case '!':
                            if (reader.Peek() == '=')
                            {
                                col++;
                                reader.Read();
                                yield return new Token(TokenKind.OptNeq, null, tokenLn, tokenCol);
                            }
                            else
                            {
                                yield return new Token(TokenKind.KwdNot, null, tokenLn, tokenCol);
                            }
                            break;
                        case '=':
                            if (reader.Peek() == '=')
                            {
                                col++;
                                reader.Read();
                                yield return new Token(TokenKind.OptEql, null, tokenLn, tokenCol);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptAssign, null, tokenLn, tokenCol);
                            }
                            break;

                        default:
                            throw new NuaLexException($"Invalid character '{ch}'");
                    }
                }
            }
        }
    }
}
