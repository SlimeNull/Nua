using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nua.CompileService;


public class Lexer
{
    private readonly TextReader _source;
    private LexStatus _status;

    public TextReader Source => _source;
    public LexStatus Status => _status;

    public Lexer(TextReader source)
    {
        _source = source;
        _status = new();
    }

    public IEnumerable<Token> Lex()
    {
        int ln = 0;
        int col = 0;
        int index = 0;
        _status = new();

        int ReadAndRise()
        {
            int result = _source.Read();

            col++;
            index++;

            if (result == '\r')
            {
                var next = _source.Peek();
                if (next == '\n')
                    _source.Read();

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
                    ch = _source.Peek();
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
                    yield return new Token(new Range(startIndex, index), kwd, null, tokenLn, tokenCol);
                else
                    yield return new Token(new Range(startIndex, index), TokenKind.Identifier, sb.ToString(), tokenLn, tokenCol);

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

                switch (_source.Peek())
                {
                    case 'x':
                        _source.Read();
                        isHex = true;
                        break;
                    case 'b':
                        _source.Read();
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
                        yield return new Token(new Range(startIndex, index), TokenKind.Number, num.ToString(), tokenLn, tokenCol);
                    }
                    else
                    {
                        _status.Errors.Add(new(new Range(startIndex, index), "Invalid number"));
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
                        yield return new Token(new Range(startIndex, index), TokenKind.Number, num.ToString(), tokenLn, tokenCol);
                    }
                    else
                    {
                        _status.Errors.Add(new(new Range(startIndex, index), "Invalid number"));
                    }
                    continue;
                }

                while (ch != -1)
                {
                    ch = _source.Peek();
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
                        ch = _source.Peek();
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

                    ch = _source.Peek();
                    if (ch >= '0' || ch <= '9' || ch == '+' || ch == '-')
                    {
                        sb.Append((char)ch);
                        ReadAndRise();
                    }

                    while (true)
                    {
                        ch = _source.Peek();
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
                {
                    _status.Errors.Add(new(new Range(startIndex, index), "Invalid number"));
                }

                yield return new Token(new Range(startIndex, index), TokenKind.Number, numberStr, tokenLn, tokenCol);
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
                    {
                        _status.Errors.Add(new(new Range(startIndex, index), "String not closed"));

                        if (nextCh == -1)
                            break;
                    }

                    var nextCch = (char)nextCh;
                    if (nextCch == '\\')
                    {
                        var escapeSeq = ReadAndRise();

                        if (escapeSeq == -1)
                        {
                            _status.Errors.Add(new(new Range(startIndex, index), "String not closed"));
                            break;
                        }

                        char? escaped = escapeSeq switch
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
                            _ => null
                        };

                        if (escaped.HasValue)
                        {
                            sb.Append(escaped.Value);
                        }
                        else
                        {
                            _status.Errors.Add(new(new Range(index, index), "Invalid escape sequence"));
                        }
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

                yield return new Token(new Range(startIndex, index), TokenKind.String, sb.ToString(), tokenLn, tokenCol);
            }
            else if (cch == '#')
            {
                while (true)
                {
                    var nextCh = _source.Peek();

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
                        if (_source.Peek() == '+')
                        {
                            ReadAndRise();
                            yield return new Token(new Range(startIndex, index), TokenKind.OptDoubleAdd, null, tokenLn, tokenCol);
                        }
                        else if (_source.Peek() == '=')
                        {
                            ReadAndRise();
                            yield return new Token(new Range(startIndex, index), TokenKind.OptAddWith, null, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(new Range(startIndex, index), TokenKind.OptAdd, null, tokenLn, tokenCol);
                        }
                        break;
                    case '-':
                        if (_source.Peek() == '-')
                        {
                            ReadAndRise();
                            yield return new Token(new Range(startIndex, index), TokenKind.OptDoubleMin, null, tokenLn, tokenCol);
                        }
                        else if (_source.Peek() == '=')
                        {
                            ReadAndRise();
                            yield return new Token(new Range(startIndex, index), TokenKind.OptMinWith, null, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(new Range(startIndex, index), TokenKind.OptMin, null, tokenLn, tokenCol);
                        }
                        break;
                    case '*':
                        if (_source.Peek() == '*')
                        {
                            ReadAndRise();
                            yield return new Token(new Range(startIndex, index), TokenKind.OptPow, null, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(new Range(startIndex, index), TokenKind.OptMul, null, tokenLn, tokenCol);
                        }
                        break;
                    case '/':
                        if (_source.Peek() == '/')
                        {
                            ReadAndRise();
                            yield return new Token(new Range(startIndex, index), TokenKind.OptDivInt, null, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(new Range(startIndex, index), TokenKind.OptDiv, null, tokenLn, tokenCol);
                        }
                        break;
                    case '%':
                        yield return new Token(new Range(startIndex, index), TokenKind.OptMod, null, tokenLn, tokenCol);
                        break;
                    case '(':
                        yield return new Token(new Range(startIndex, index), TokenKind.ParenthesesLeft, null, tokenLn, tokenCol);
                        break;
                    case ')':
                        yield return new Token(new Range(startIndex, index), TokenKind.ParenthesesRight, null, tokenLn, tokenCol);
                        break;
                    case '[':
                        yield return new Token(new Range(startIndex, index), TokenKind.SquareBracketLeft, null, tokenLn, tokenCol);
                        break;
                    case ']':
                        yield return new Token(new Range(startIndex, index), TokenKind.SquareBracketRight, null, tokenLn, tokenCol);
                        break;
                    case '{':
                        yield return new Token(new Range(startIndex, index), TokenKind.BigBracketLeft, null, tokenLn, tokenCol);
                        break;
                    case '}':
                        yield return new Token(new Range(startIndex, index), TokenKind.BigBracketRight, null, tokenLn, tokenCol);
                        break;
                    case ':':
                        yield return new Token(new Range(startIndex, index), TokenKind.OptColon, null, tokenLn, tokenCol);
                        break;
                    case ',':
                        yield return new Token(new Range(startIndex, index), TokenKind.OptComma, null, tokenLn, tokenCol);
                        break;
                    case '.':
                        yield return new Token(new Range(startIndex, index), TokenKind.OptDot, null, tokenLn, tokenCol);
                        break;
                    case '>':
                        if (_source.Peek() == '=')
                        {
                            ReadAndRise();
                            yield return new Token(new Range(startIndex, index), TokenKind.OptGeq, null, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(new Range(startIndex, index), TokenKind.OptGtr, null, tokenLn, tokenCol);
                        }
                        break;
                    case '<':
                        if (_source.Peek() == '=')
                        {
                            ReadAndRise();
                            yield return new Token(new Range(startIndex, index), TokenKind.OptLeq, null, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(new Range(startIndex, index), TokenKind.OptLss, null, tokenLn, tokenCol);
                        }
                        break;
                    case '!':
                        if (_source.Peek() == '=')
                        {
                            ReadAndRise();
                            yield return new Token(new Range(startIndex, index), TokenKind.OptNeq, null, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(new Range(startIndex, index), TokenKind.KwdNot, null, tokenLn, tokenCol);
                        }
                        break;
                    case '=':
                        if (_source.Peek() == '=')
                        {
                            ReadAndRise();
                            yield return new Token(new Range(startIndex, index), TokenKind.OptEql, null, tokenLn, tokenCol);
                        }
                        else
                        {
                            yield return new Token(new Range(startIndex, index), TokenKind.OptAssign, null, tokenLn, tokenCol);
                        }
                        break;

                    default:
                        _status.Errors.Add(new(new Range(index, index), $"Invalid character '{(char)ch}'(value {ch})"));
                        break;
                }
            }
        }
    }
}
