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
            while (true)
            {
                var ch = reader.Read();
                char cch = (char)ch;

                if (ch == -1)
                    yield break;

                if (char.IsWhiteSpace(cch))
                    continue;
                else if (char.IsLetter(cch) || cch == '_')
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(cch);

                    while (true)
                    {
                        ch = reader.Peek();
                        cch = (char)ch;

                        if (char.IsLetterOrDigit(cch) || cch == '_')
                        {
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

                        _ => null
                    };

                    if (kwdToken is TokenKind kwd)
                        yield return new Token(kwd, null);
                    else
                        yield return new Token(TokenKind.Identifier, sb.ToString());

                    continue;
                }
                else if (char.IsDigit(cch))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(cch);

                    while (ch != -1)
                    {
                        ch = reader.Peek();
                        cch = (char)ch;
                        if (ch >= '0' && ch <= '9')
                        {
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
                        ch = reader.Read();  // skip '.'
                        sb.Append(cch);
                        while (ch != -1)
                        {
                            ch = reader.Peek();
                            cch = (char)ch;
                            if (ch >= '0' && ch <= '9')
                            {
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
                        reader.Read();  // skip 'e'
                        sb.Append(cch);

                        ch = reader.Read();
                        if (ch >= '0' || ch <= '9' || ch == '+' || ch == '-')
                        {
                            reader.Read();
                            sb.Append((char)ch);
                        }

                        while (ch != -1)
                        {
                            ch = reader.Peek();
                            cch = (char)ch;
                            if (ch >= '0' && ch <= '9')
                            {
                                reader.Read();
                                sb.Append(cch);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    yield return new Token(TokenKind.Number, sb.ToString());
                    continue;
                }
                else if (cch == '"')
                {
                    StringBuilder sb = new();

                    while (true)
                    {
                        var nextCh = reader.Read();

                        if (nextCh == -1)
                            throw new NuaLexException("String not closed");

                        var nextCch = (char)nextCh;
                        if (nextCch == '\\')
                        {
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

                    yield return new Token(TokenKind.String, sb.ToString());
                }
                else
                {
                    switch (ch)
                    {
                        case '+':
                            if (reader.Peek() == '+')
                            {
                                reader.Read();
                                yield return new Token(TokenKind.OptDoubleAdd, null);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptAdd, null);
                            }
                            break;
                        case '-':
                            if (reader.Peek() == '-')
                            {
                                reader.Read();
                                yield return new Token(TokenKind.OptDoubleMin, null);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptMin, null);
                            }
                            break;
                        case '*':
                            if (reader.Peek() == '*')
                            {
                                reader.Read();
                                yield return new Token(TokenKind.OptPow, null);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptMul, null);
                            }
                            break;
                        case '/':
                            if (reader.Peek() == '/')
                            {
                                reader.Read();
                                yield return new Token(TokenKind.OptDivInt, null);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptDiv, null);
                            }
                            break;
                        case '%':
                            yield return new Token(TokenKind.OptMod, null);
                            break;
                        case '(':
                            yield return new Token(TokenKind.ParenthesesLeft, null);
                            break;
                        case ')':
                            yield return new Token(TokenKind.ParenthesesRight, null);
                            break;
                        case '[':
                            yield return new Token(TokenKind.SquareBracketLeft, null);
                            break;
                        case ']':
                            yield return new Token(TokenKind.SquareBracketRight, null);
                            break;
                        case '{':
                            yield return new Token(TokenKind.BigBracketLeft, null);
                            break;
                        case '}':
                            yield return new Token(TokenKind.BigBracketRight, null);
                            break;
                        case ':':
                            yield return new Token(TokenKind.OptColon, null);
                            break;
                        case ',':
                            yield return new Token(TokenKind.OptComma, null);
                            break;
                        case '.':
                            yield return new Token(TokenKind.OptDot, null);
                            break;
                        case '>':
                            if (reader.Peek() == '=')
                            {
                                reader.Read();
                                yield return new Token(TokenKind.OptGeq, null);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptGtr, null);
                            }
                            break;
                        case '<':
                            if (reader.Peek() == '=')
                            {
                                reader.Read();
                                yield return new Token(TokenKind.OptLeq, null);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptLss, null);
                            }
                            break;
                        case '!':
                            if (reader.Peek() == '=')
                            {
                                reader.Read();
                                yield return new Token(TokenKind.OptNeq, null);
                            }
                            else
                            {
                                yield return new Token(TokenKind.KwdNot, null);
                            }
                            break;
                        case '=':
                            if (reader.Peek() == '=')
                            {
                                reader.Read();
                                yield return new Token(TokenKind.OptEql, null);
                            }
                            else
                            {
                                yield return new Token(TokenKind.OptAssign, null);
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
