using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nua.CompileService;


public class Lexer
{
    public static IEnumerable<Token> Lex(TextReader reader)
    {
        int ln = 0;
        int col = 0;
        int index = 0;

        int ReadAndRise()
        {
            int result = reader.Read();

            col++;
            index++;

            if (result == '\r')
            {
                var next = reader.Peek();
                if (next == '\n')
                    reader.Read();

                col = 0;
                ln++;
            }
            else if (result == '\n')
            {
                col = 0;
                ln++;
            }

            return result;
        }

        while (true)
        {
            var ch = ReadAndRise();
            char cch = (char)ch;

            if (ch == -1)
                yield break;

            if (char.IsWhiteSpace(cch))
            {
                continue;
            }
            else if (char.IsLetter(cch) || cch == '_')
            {
                int tokenLn = ln;
                int tokenCol = col - 1;
                int startIndex = index - 1;

                StringBuilder sb = new StringBuilder();
                sb.Append(cch);

                while (true)
                {
                    ch = reader.Peek();
                    cch = (char)ch;

                    if (char.IsLetterOrDigit(cch) || cch == '_')
                    {
                        ReadAndRise();
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
                    yield return new Token(kwd, null, startIndex, index, tokenLn, tokenCol);
                else
                    yield return new Token(TokenKind.Identifier, sb.ToString(), startIndex, index, tokenLn, tokenCol);

                continue;
            }
            else if (char.IsAsciiDigit(cch))
            {
                // 不支持 .123
                // 支持 1.e10
                int tokenLn = ln;
                int tokenCol = col - 1;
                int startIndex = index - 1;

                bool isHex = false;
                bool isBit = false;

                StringBuilder sb = new();

                switch (reader.Peek())
                {
                    case 'x':
                        reader.Read();
                        isHex = true;
                        break;
                    case 'b':
                        reader.Read();
                        isBit = true;
                        break;
                    default:
                        sb.Append(cch);
                        break;
                }

                //略过小数位和指数位
                if (isHex)
                {
                    while (true)
                    {
                        ch = ReadAndRise();  // skip '.'
                        cch = (char)ch;
                        if (char.IsAsciiHexDigit(cch))
                        {
                            sb.Append(cch);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (ulong.TryParse(sb.ToString(), System.Globalization.NumberStyles.HexNumber, null, out var num))
                    {
                        yield return new Token(TokenKind.Number, num.ToString(), startIndex, index, tokenLn, tokenCol);
                    }
                    else
                    {
                        throw new NuaLexException("Invalid number");
                    }
                    continue;
                }
                else if (isBit)
                {
                    while (true)
                    {
                        ch = ReadAndRise();  // skip '.'
                        cch = (char)ch;
                        if (cch is '0' or '1')
                        {
                            sb.Append(cch);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (ulong.TryParse(sb.ToString(), System.Globalization.NumberStyles.BinaryNumber, null, out var num))
                    {
                        yield return new Token(TokenKind.Number, num.ToString(), startIndex, index, tokenLn, tokenCol);
                    }
                    else
                    {
                        throw new NuaLexException("Invalid number");
                    }
                    continue;
                }

                while (ch != -1)
                {
                    ch = reader.Peek();
                    cch = (char)ch;
                    if (char.IsAsciiDigit(cch))
                    {
                        ReadAndRise();
                        sb.Append(cch);
                    }
                    else
                    {
                        break;
                    }
                }

                if (cch == '.')
                {
                    ch = ReadAndRise();  // skip '.'
                    sb.Append(cch);
                    while (ch != -1)
                    {
                        ch = reader.Peek();
                        cch = (char)ch;
                        if (char.IsAsciiDigit(cch))
                        {
                            ReadAndRise();
                            sb.Append(cch);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (cch is 'e' or 'E')
                {
                    ReadAndRise();  // skip 'e'
                    sb.Append(cch);

                    ch = reader.Peek();
                    if (ch >= '0' || ch <= '9' || ch == '+' || ch == '-')
                    {
                        sb.Append((char)ch);
                        ReadAndRise();
                    }

                    while (true)
                    {
                        ch = reader.Peek();
                        cch = (char)ch;
                        if (ch >= '0' && ch <= '9')
                        {
                            ReadAndRise();
                            sb.Append(cch);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                string numberStr = sb.ToString();

                if (!double.TryParse(numberStr, out _))
                    throw new NuaLexException("Invalid number");

                yield return new Token(TokenKind.Number, numberStr, startIndex, index, tokenLn, tokenCol);
                continue;
            }
            else if (cch == '"')
            {
                int tokenLn = ln;
                int tokenCol = col - 1;
                int startIndex = index - 1;

                StringBuilder sb = new();

                while (true)
                {
                    var nextCh = ReadAndRise();

                    if (nextCh is -1 or '\r' or '\n')
                        throw new NuaLexException("String not closed");

                    var nextCch = (char)nextCh;
                    if (nextCch == '\\')
                    {
                        var escapeSeq = ReadAndRise();

                        if (escapeSeq == -1)
                            throw new NuaLexException("String not closed");

                        sb.Append(escapeSeq switch
                        {
                            '"' => '"',
                            '\\' => '\\',
                            '\'' => '\'',
                            'r' => '\r',
                            'n' => '\n',
                            '0' => '\0', // NUL
                            'b' => '\b', // 退格
                            't' => '\t', // 水平制表符
                            'v' => '\v', // 垂直制表符
                            'f' => '\f', // 换页
                            'a' => '\a', // 响铃
                            'e' => '\x1B', // <ESCAPE>
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

                yield return new Token(TokenKind.String, sb.ToString(), startIndex, index, tokenLn, tokenCol);
            }
            else if (cch == '#')
            {
                while (true)
                {
                    var nextCh = reader.Peek();

                    if (nextCh is '\r' or '\n' or -1)
                        break;

                    ReadAndRise();
                }
            }
            else
            {
                int tokenLn = ln;
                int tokenCol = col - 1;
                int startIndex = index - 1;

                switch (ch)
                {
                    case '+':
                        if (reader.Peek() == '+')
                        {
                            ReadAndRise();
                            yield return new Token(TokenKind.OptDoubleAdd, null, startIndex, index, tokenLn, tokenCol);
                        }
                        else if (reader.Peek() == '=')
                        {
                            ReadAndRise();
                            yield return new Token(TokenKind.OptAddWith, null, startIndex, index, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(TokenKind.OptAdd, null, startIndex, index, tokenLn, tokenCol);
                        }
                        break;
                    case '-':
                        if (reader.Peek() == '-')
                        {
                            ReadAndRise();
                            yield return new Token(TokenKind.OptDoubleMin, null, startIndex, index, tokenLn, tokenCol);
                        }
                        else if (reader.Peek() == '=')
                        {
                            ReadAndRise();
                            yield return new Token(TokenKind.OptMinWith, null, startIndex, index, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(TokenKind.OptMin, null, startIndex, index, tokenLn, tokenCol);
                        }
                        break;
                    case '*':
                        if (reader.Peek() == '*')
                        {
                            ReadAndRise();
                            yield return new Token(TokenKind.OptPow, null, startIndex, index, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(TokenKind.OptMul, null, startIndex, index, tokenLn, tokenCol);
                        }
                        break;
                    case '/':
                        if (reader.Peek() == '/')
                        {
                            ReadAndRise();
                            yield return new Token(TokenKind.OptDivInt, null, startIndex, index, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(TokenKind.OptDiv, null, startIndex, index, tokenLn, tokenCol);
                        }
                        break;
                    case '%':
                        yield return new Token(TokenKind.OptMod, null, startIndex, index, tokenLn, tokenCol);
                        break;
                    case '(':
                        yield return new Token(TokenKind.ParenthesesLeft, null, startIndex, index, tokenLn, tokenCol);
                        break;
                    case ')':
                        yield return new Token(TokenKind.ParenthesesRight, null, startIndex, index, tokenLn, tokenCol);
                        break;
                    case '[':
                        yield return new Token(TokenKind.SquareBracketLeft, null, startIndex, index, tokenLn, tokenCol);
                        break;
                    case ']':
                        yield return new Token(TokenKind.SquareBracketRight, null, startIndex, index, tokenLn, tokenCol);
                        break;
                    case '{':
                        yield return new Token(TokenKind.BigBracketLeft, null, startIndex, index, tokenLn, tokenCol);
                        break;
                    case '}':
                        yield return new Token(TokenKind.BigBracketRight, null, startIndex, index, tokenLn, tokenCol);
                        break;
                    case ':':
                        yield return new Token(TokenKind.OptColon, null, startIndex, index, tokenLn, tokenCol);
                        break;
                    case ',':
                        yield return new Token(TokenKind.OptComma, null, startIndex, index, tokenLn, tokenCol);
                        break;
                    case '.':
                        yield return new Token(TokenKind.OptDot, null, startIndex, index, tokenLn, tokenCol);
                        break;
                    case '>':
                        if (reader.Peek() == '=')
                        {
                            ReadAndRise();
                            yield return new Token(TokenKind.OptGeq, null, startIndex, index, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(TokenKind.OptGtr, null, startIndex, index, tokenLn, tokenCol);
                        }
                        break;
                    case '<':
                        if (reader.Peek() == '=')
                        {
                            ReadAndRise();
                            yield return new Token(TokenKind.OptLeq, null, startIndex, index, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(TokenKind.OptLss, null, startIndex, index, tokenLn, tokenCol);
                        }
                        break;
                    case '!':
                        if (reader.Peek() == '=')
                        {
                            ReadAndRise();
                            yield return new Token(TokenKind.OptNeq, null, startIndex, index, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(TokenKind.KwdNot, null, startIndex, index, tokenLn, tokenCol);
                        }
                        break;
                    case '=':
                        if (reader.Peek() == '=')
                        {
                            ReadAndRise();
                            yield return new Token(TokenKind.OptEql, null, startIndex, index, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(TokenKind.OptAssign, null, startIndex, index, tokenLn, tokenCol);
                        }
                        break;

                    default:
                        throw new NuaLexException($"Invalid character '{ch}'");
                }
            }
        }
    }
}
