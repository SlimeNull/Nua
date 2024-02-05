using System.Runtime.CompilerServices;
using System.Text;
using Nua.CompileService;
using Nua.CompileService.Syntaxes;
using PrettyPrompt;
using PrettyPrompt.Consoles;
using PrettyPrompt.Highlighting;

namespace NuaConsole
{
    public class NuaReplPromptCallbacks : PromptCallbacks
    {
        private readonly int _tabSize;

        public NuaReplPromptCallbacks(int tabSize = 2)
        {
            _tabSize = tabSize;
        }

        static readonly AnsiColor idColor = AnsiColor.Rgb(156, 220, 254);
        static readonly AnsiColor kwdColor = AnsiColor.Rgb(197, 134, 192);
        static readonly AnsiColor kwdFuncColor = AnsiColor.Rgb(86, 156, 214);
        static readonly AnsiColor numberColor = AnsiColor.Rgb(181, 206, 168);
        static readonly AnsiColor stringColor = AnsiColor.Rgb(206, 145, 120);
        static readonly AnsiColor boolAndNullColor = AnsiColor.Rgb(79, 193, 255);
        static readonly AnsiColor operatorColor = AnsiColor.Rgb(180, 180, 180);
        static readonly AnsiColor bracketColor = AnsiColor.Rgb(220, 220, 220);


        readonly Dictionary<TokenKind, ConsoleFormat> tokenKindColors = new()
        {
            [TokenKind.None] = ConsoleFormat.None,
            [TokenKind.KwdRequire] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdIf] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdElse] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdElif] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdFor] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdIn] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdOf] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdLoop] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdWhile] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdContinue] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdBreak] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdNull] = new ConsoleFormat(boolAndNullColor),
            [TokenKind.KwdTrue] = new ConsoleFormat(boolAndNullColor),
            [TokenKind.KwdFalse] = new ConsoleFormat(boolAndNullColor),
            [TokenKind.KwdNot] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdAnd] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdOr] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdFunction] = new ConsoleFormat(kwdFuncColor),
            [TokenKind.KwdReturn] = new ConsoleFormat(kwdColor),
            [TokenKind.KwdGlobal] = new ConsoleFormat(kwdColor),
            [TokenKind.OptColon] = new ConsoleFormat(operatorColor),
            [TokenKind.OptComma] = new ConsoleFormat(operatorColor),
            [TokenKind.OptDot] = new ConsoleFormat(operatorColor),
            [TokenKind.OptAdd] = new ConsoleFormat(operatorColor),
            [TokenKind.OptMin] = new ConsoleFormat(operatorColor),
            [TokenKind.OptMul] = new ConsoleFormat(operatorColor),
            [TokenKind.OptDiv] = new ConsoleFormat(operatorColor),
            [TokenKind.OptMod] = new ConsoleFormat(operatorColor),
            [TokenKind.OptAddWith] = new ConsoleFormat(operatorColor),
            [TokenKind.OptMinWith] = new ConsoleFormat(operatorColor),
            [TokenKind.OptPow] = new ConsoleFormat(operatorColor),
            [TokenKind.OptDivInt] = new ConsoleFormat(operatorColor),
            [TokenKind.OptDoubleAdd] = new ConsoleFormat(operatorColor),
            [TokenKind.OptDoubleMin] = new ConsoleFormat(operatorColor),
            [TokenKind.OptEql] = new ConsoleFormat(operatorColor),
            [TokenKind.OptNeq] = new ConsoleFormat(operatorColor),
            [TokenKind.OptLss] = new ConsoleFormat(operatorColor),
            [TokenKind.OptLeq] = new ConsoleFormat(operatorColor),
            [TokenKind.OptGtr] = new ConsoleFormat(operatorColor),
            [TokenKind.OptGeq] = new ConsoleFormat(operatorColor),
            [TokenKind.OptAssign] = new ConsoleFormat(operatorColor),
            [TokenKind.ParenthesesLeft] = new ConsoleFormat(bracketColor),
            [TokenKind.ParenthesesRight] = new ConsoleFormat(bracketColor),
            [TokenKind.SquareBracketLeft] = new ConsoleFormat(bracketColor),
            [TokenKind.SquareBracketRight] = new ConsoleFormat(bracketColor),
            [TokenKind.BigBracketLeft] = new ConsoleFormat(bracketColor),
            [TokenKind.BigBracketRight] = new ConsoleFormat(bracketColor),
            [TokenKind.Identifier] = new ConsoleFormat(idColor),
            [TokenKind.String] = new ConsoleFormat(stringColor),
            [TokenKind.Number] = new ConsoleFormat(numberColor),
        };

        IList<Token>? _tokens;
        Expr? _expr;
        bool _isCompleteExpr = true;

        protected override async Task<KeyPress> TransformKeyPressAsync(string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
        {
            if (keyPress.ConsoleKeyInfo.Key == ConsoleKey.Enter &&
                keyPress.ConsoleKeyInfo.Modifiers == default &&
                !string.IsNullOrWhiteSpace(text))
            {
                if (!_isCompleteExpr)
                    return NewLineWithIndentation(GetSmartIndentationLevel(text, caret));
            }

            return await base.TransformKeyPressAsync(text, caret, keyPress, cancellationToken);

            static KeyPress NewLineWithIndentation(int indentation) =>
                new(ConsoleKey.Insert.ToKeyInfo('\0', shift: true), "\n" + new string('\t', indentation));
        }


        protected override Task<(string Text, int Caret)> FormatInput(string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
        {
            var reader = new StringReader(text);

            _tokens = Lexer.Lex(reader).ToList();
            _expr = null;

            try
            {
                _expr = Parser.Parse(_tokens);
                _isCompleteExpr = true;
            }
            catch (NuaParseException parseException)
            {
                _isCompleteExpr = !parseException.Status.RequireMoreTokens;
            }

            var keyChar = keyPress.ConsoleKeyInfo.KeyChar;
            
            if (caret > 0)
            {
                switch (keyChar)
                {
                    case '{' or '}':
                    {
                        StringBuilder result = new();
                        int newCaret = -1;

                        bool inSpaceAfterNewLine = false;
                        for (int i = 0; i < text.Length; i++)
                        {
                            char ch = text[i];

                            if (inSpaceAfterNewLine)
                            {
                                if (!char.IsWhiteSpace(ch))
                                {
                                    int indentLevel = GetSmartIndentationLevel(text, i);

                                    if (ch == '}')
                                        indentLevel -= 1;

                                    if (indentLevel < 0)
                                        indentLevel = 0;

                                    result.Append(' ', _tabSize * indentLevel);
                                    inSpaceAfterNewLine = false;
                                    result.Append(ch);
                                }
                            }
                            else
                            {
                                if (ch == '\n')
                                    inSpaceAfterNewLine = true;

                                result.Append(ch);
                            }

                            if (i == caret)
                                newCaret = result.Length - 1;
                        }

                        if (newCaret == -1)
                            newCaret = result.Length;

                        text = result.ToString();
                        caret = newCaret;
                        break;
                    }
                    default:
                        break;
                }
            }

            return base.FormatInput(text, caret, keyPress, cancellationToken);
        }

        protected override Task<IReadOnlyCollection<FormatSpan>> HighlightCallbackAsync(string text, CancellationToken cancellationToken)
        {
            List<FormatSpan> result = new List<FormatSpan>();

            try
            {
                if (_tokens != null)
                {
                    foreach (var token in _tokens)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        result.Add(FormatSpan.FromBounds(token.StartIndex, token.EndIndex, tokenKindColors[token.Kind]));
                    }
                }
            }
            catch { }

            return Task.FromResult<IReadOnlyCollection<FormatSpan>>(result);
        }

        static int GetSmartIndentationLevel(string text, int caret)
        {
            int openBraces = 0;
            var end = Math.Min(text.Length, caret);
            for (int i = 0; i < end; i++)
            {
                var c = text[i];
                if (c == '{')
                    ++openBraces;
                if (c == '}')
                    --openBraces;
            }
            return openBraces;
        }
    }
}
