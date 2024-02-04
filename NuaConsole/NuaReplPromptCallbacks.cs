using System.Runtime.CompilerServices;
using System.Text;
using Nua.CompileService;
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
            [TokenKind.OptColon] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptComma] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptDot] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptAdd] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptMin] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptMul] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptDiv] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptMod] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptAddWith] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptMinWith] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptPow] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptDivInt] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptDoubleAdd] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptDoubleMin] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptEql] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptNeq] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptLss] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptLeq] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptGtr] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptGeq] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.OptAssign] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.ParenthesesLeft] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.ParenthesesRight] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.SquareBracketLeft] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.SquareBracketRight] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.BigBracketLeft] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.BigBracketRight] = new ConsoleFormat(AnsiColor.White),
            [TokenKind.Identifier] = new ConsoleFormat(idColor),
            [TokenKind.String] = new ConsoleFormat(stringColor),
            [TokenKind.Number] = new ConsoleFormat(numberColor),
        };

        protected override async Task<KeyPress> TransformKeyPressAsync(string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
        {
            if (keyPress.ConsoleKeyInfo.Key == ConsoleKey.Enter &&
                keyPress.ConsoleKeyInfo.Modifiers == default)
            {
                bool isCompleteExpr;

                try
                {
                    var reader = new StringReader(text);
                    var tokens = Lexer.Lex(reader).ToArray();
                    Parser.Parse(tokens);

                    isCompleteExpr = true;
                }
                catch (NuaParseException parseException)
                {
                    isCompleteExpr = !parseException.Status.RequireMoreTokens;
                }

                if (!isCompleteExpr)
                    return NewLineWithIndentation(GetSmartIndentationLevel(text, caret));
            }

            return await base.TransformKeyPressAsync(text, caret, keyPress, cancellationToken);

            static KeyPress NewLineWithIndentation(int indentation) =>
                new(ConsoleKey.Insert.ToKeyInfo('\0', shift: true), "\n" + new string('\t', indentation));
        }


        protected override Task<(string Text, int Caret)> FormatInput(string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
        {
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

            var reader = new StringReader(text);
            var tokens = Lexer.Lex(reader);

            try
            {
                foreach (var token in tokens)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    result.Add(FormatSpan.FromBounds(token.StartIndex, token.EndIndex, tokenKindColors[token.Kind]));
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
