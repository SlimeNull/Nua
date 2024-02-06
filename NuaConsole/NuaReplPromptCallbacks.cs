using System.Runtime.CompilerServices;
using System.Text;
using Nua;
using Nua.CompileService;
using Nua.CompileService.Syntaxes;
using Nua.Stdlib;
using Nua.Types;
using PrettyPrompt;
using PrettyPrompt.Completion;
using PrettyPrompt.Consoles;
using PrettyPrompt.Documents;
using PrettyPrompt.Highlighting;
using static System.Net.Mime.MediaTypeNames;
using static PrettyPrompt.Highlighting.FormattedString.TextElementsEnumerator;

namespace NuaConsole
{
    public class NuaReplPromptCallbacks : PromptCallbacks
    {
        private readonly NuaContext _scriptContext;
        private readonly int _tabSize;

        public NuaReplPromptCallbacks(NuaContext scriptContext, int tabSize = 2)
        {
            _scriptContext = scriptContext;
            _tabSize = tabSize;
        }

        static readonly AnsiColor idColor = AnsiColor.Rgb(156, 220, 254);
        static readonly AnsiColor kwdColor = AnsiColor.Rgb(197, 134, 192);
        static readonly AnsiColor kwdFuncColor = AnsiColor.Rgb(86, 156, 214);
        static readonly AnsiColor numberColor = AnsiColor.Rgb(181, 206, 168);
        static readonly AnsiColor stringColor = AnsiColor.Rgb(206, 145, 120);
        static readonly AnsiColor boolAndNullColor = AnsiColor.Rgb(86, 156, 214);
        static readonly AnsiColor operatorColor = AnsiColor.Rgb(180, 180, 180);
        static readonly AnsiColor bracketColor = AnsiColor.Rgb(220, 220, 220);
        static readonly AnsiColor stdModuleNameColor = AnsiColor.Rgb(73, 201, 176);
        static readonly AnsiColor functionNameColor = AnsiColor.Rgb(220, 220, 170);


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
        bool _lexParseOk = false;

        void LexAndParse(string text)
        {
            var reader = new StringReader(text);

            _tokens = null;
            _expr = null;

            try
            {
                tokensBuffer.Clear();

                foreach (var token in Lexer.Lex(reader))
                    tokensBuffer.Add(token);

                _tokens = tokensBuffer;
                _expr = Parser.Parse(_tokens);
                _isCompleteExpr = true;
            }
            catch (NuaParseException parseException)
            {
                _isCompleteExpr = !parseException.Status.RequireMoreTokens;
            }
            catch (Exception)
            {
                _isCompleteExpr = true;
            }

            _lexParseOk = true;
        }

        protected override async Task<KeyPress> TransformKeyPressAsync(string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
        {
            if (keyPress.ConsoleKeyInfo.Key == ConsoleKey.Enter &&
                keyPress.ConsoleKeyInfo.Modifiers == default &&
                !string.IsNullOrWhiteSpace(text))
            {
                if (!_isCompleteExpr)
                    return NewLineWithIndentation(GetSmartIndentationLevel(text, caret));
            }

            _lexParseOk = false;

            return await base.TransformKeyPressAsync(text, caret, keyPress, cancellationToken);

            static KeyPress NewLineWithIndentation(int indentation) =>
                new(ConsoleKey.Insert.ToKeyInfo('\0', shift: true), "\n" + new string('\t', indentation));
        }

        readonly List<Token> tokensBuffer = new();
        protected override Task<(string Text, int Caret)> FormatInput(string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
        {
            if (!_lexParseOk)
                LexAndParse(text);

            var keyChar = keyPress.ConsoleKeyInfo.KeyChar;

            if (keyChar is '{' or '[' or '(' or '}' or ']' or ')')
            {
                StringBuilder result = new();
                StringBuilder spaceBuffer = new();
                int newCaret = -1;

                bool inSpaceAfterNewLine = false;
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];

                    if (inSpaceAfterNewLine)
                    {
                        if (!char.IsWhiteSpace(ch) || ch == '\n')
                        {
                            int indentLevel = GetSmartIndentationLevel(text, i);

                            if (ch is '}' or ']' or ')')
                                indentLevel -= 1;

                            if (indentLevel < 0)
                                indentLevel = 0;

                            result.Append(' ', _tabSize * indentLevel);
                            inSpaceAfterNewLine = false;
                            result.Append(ch);

                            _lexParseOk = spaceBuffer == result;
                        }

                        spaceBuffer.Append(ch);
                    }
                    else
                    {
                        if (ch == '\n')
                        {
                            inSpaceAfterNewLine = true;
                            spaceBuffer.Clear();
                        }

                        result.Append(ch);
                    }

                    if (i == caret)
                        newCaret = result.Length - 1;
                }

                if (newCaret == -1)
                    newCaret = result.Length;

                text = result.ToString();
                caret = newCaret;
            }

            return base.FormatInput(text, caret, keyPress, cancellationToken);
        }

        readonly Dictionary<Range, ConsoleFormat> _highlightBuffer = new();
        protected override Task<IReadOnlyCollection<FormatSpan>> HighlightCallbackAsync(string text, CancellationToken cancellationToken)
        {
            if (!_lexParseOk)
                LexAndParse(text);

            _highlightBuffer.Clear();

            try
            {
                if (_tokens is not null)
                {
                    foreach (var token in _tokens)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        _highlightBuffer[new Range(token.StartIndex, token.EndIndex)] = tokenKindColors[token.Kind];
                    }
                }

                if (_expr is not null)
                {
                    foreach (var syntax in _expr.TreeEnumerate())
                    {
                        if (syntax is ValueAccessExpr valueAccessExpr)
                        {
                            if (valueAccessExpr.ValueExpr is VariableExpr variableExpr &&
                                variableExpr.NameToken.HasValue)
                            {
                                if (valueAccessExpr.TailExpr is ValueInvokeAccessTailExpr)
                                {
                                    _highlightBuffer[new Range(variableExpr.NameToken.Value.StartIndex, variableExpr.NameToken.Value.EndIndex)] = new ConsoleFormat(functionNameColor);
                                }
                            }

                            ValueAccessTailExpr lastMemberAccessTail = valueAccessExpr.TailExpr;
                            while (lastMemberAccessTail.NextTailExpr is ValueMemberAccessTailExpr)
                                lastMemberAccessTail = lastMemberAccessTail.NextTailExpr;

                            if (lastMemberAccessTail is ValueMemberAccessTailExpr memberAccessTailExpr &&
                                lastMemberAccessTail.NextTailExpr is null &&
                                memberAccessTailExpr.NameToken.HasValue)
                            {
                                var value = valueAccessExpr.Evaluate(_scriptContext);

                                if (value is NuaFunction)
                                    _highlightBuffer[new Range(memberAccessTailExpr.NameToken.Value.StartIndex, memberAccessTailExpr.NameToken.Value.EndIndex)] = new ConsoleFormat(functionNameColor);
                                else if (value is StandardModuleTable)
                                    _highlightBuffer[new Range(memberAccessTailExpr.NameToken.Value.StartIndex, memberAccessTailExpr.NameToken.Value.EndIndex)] = new ConsoleFormat(stdModuleNameColor);
                            }
                        }
                        else if (syntax is ValueMemberAccessTailExpr valueAccessTailExpr &&
                            valueAccessTailExpr.NameToken.HasValue)
                        {
                            if (valueAccessTailExpr.NextTailExpr is ValueInvokeAccessTailExpr)
                            {
                                _highlightBuffer[new Range(valueAccessTailExpr.NameToken.Value.StartIndex, valueAccessTailExpr.NameToken.Value.EndIndex)] = new ConsoleFormat(functionNameColor);
                            }
                        }
                        else if (syntax is VariableExpr variableExpr)
                        {
                            if (variableExpr.NameToken.HasValue)
                            {
                                NuaValue? varibaleValue = variableExpr.Evaluate(_scriptContext);
                                if (varibaleValue is NuaFunction)
                                    _highlightBuffer[new Range(variableExpr.NameToken.Value.StartIndex, variableExpr.NameToken.Value.EndIndex)] = new ConsoleFormat(functionNameColor);
                                else if (varibaleValue is StandardModuleTable)
                                    _highlightBuffer[new Range(variableExpr.NameToken.Value.StartIndex, variableExpr.NameToken.Value.EndIndex)] = new ConsoleFormat(stdModuleNameColor);
                            }
                        }
                    }
                }
            }
            catch { }

            var result = _highlightBuffer
                .Select(kv => FormatSpan.FromBounds(kv.Key.Start.Value, kv.Key.End.Value, kv.Value))
                .ToList();

            return Task.FromResult<IReadOnlyCollection<FormatSpan>>(result);
        }

        protected override Task<bool> ShouldOpenCompletionWindowAsync(string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
        {
            if (!_lexParseOk)
                LexAndParse(text);

            if (_tokens is not null)
                foreach (var token in _tokens)
                    if (token.Kind == TokenKind.Identifier && token.StartIndex <= caret && token.EndIndex + 1 >= caret)
                        return Task.FromResult(true);

            return Task.FromResult(false);
        }

        readonly (TokenKind TokenKind, string Value, string Description)[] normalKeywords = [
            (TokenKind.KwdIf, "if", "Execute a block of code if a certain condition is true."),
            (TokenKind.KwdElif, "elif", "Check for an additional condition if the previous conditions in the if statement are false."),
            (TokenKind.KwdElse, "else", "Specify a default block of code that should be executed if none of the previous conditions in the if statement or elif statements are true."),
            (TokenKind.KwdFor, "for", "Iterate over a collection of items or perform a numerical loop."),
            (TokenKind.KwdIn, "in", "With the 'for' statement, specify the iterable object to iterate over."),
            (TokenKind.KwdOf, "of", "Specify the range of numbers in a loop."),
            (TokenKind.KwdWhile, "while", "Create a loop that executes code as long as a specified condition is true."),
            (TokenKind.KwdContinue, "continue", "Skip the remaining code in the current iteration and move on to the next iteration."),
            (TokenKind.KwdBreak, "break", "Immediately exit the loop and continue with the program execution outside of it."),
            (TokenKind.KwdNull, "null", "Represents null value."),
            (TokenKind.KwdTrue, "true", "Represents logical true value."),
            (TokenKind.KwdFalse, "false", "Represents logical false value."),
            (TokenKind.KwdNot, "not", "Performs logical negation."),
            (TokenKind.KwdAnd, "and", "Performs logical conjunction."),
            (TokenKind.KwdOr, "or", "Performs logical disjunction."),
            (TokenKind.KwdFunction, "func", "Used to create functions."),
            (TokenKind.KwdReturn, "return", "Used to exit a function execution and optionally return a value."),
            (TokenKind.KwdGlobal, "global", "Declares a variable as global within the function body."),
        ];
        protected override Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(string text, int caret, TextSpan spanToBeReplaced, CancellationToken cancellationToken)
        {
            List<CompletionItem> result = new List<CompletionItem>();

            foreach (var kwd in normalKeywords)
                result.Add(new CompletionItem(kwd.Value, new FormattedString(kwd.Value, tokenKindColors[kwd.TokenKind]), getExtendedDescription: token => Task.FromResult<FormattedString>(kwd.Description)));
            foreach (var variable in _scriptContext.GlobalFrame.Variables)
                result.Add(new CompletionItem(variable.Key, new FormattedString(variable.Key, GetVariableFormat(variable.Value)), getExtendedDescription: token => Task.FromResult<FormattedString>(variable.Value.ToString() ?? "null")));

            return Task.FromResult<IReadOnlyList<CompletionItem>>(result);
        }

        protected override Task<bool> ConfirmCompletionCommit(string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
        {
            _lexParseOk = false;
            return base.ConfirmCompletionCommit(text, caret, keyPress, cancellationToken);
        }

        ConsoleFormat GetVariableFormat(NuaValue? varibaleValue)
        {
            if (varibaleValue is NuaFunction)
                return new ConsoleFormat(functionNameColor);
            else if (varibaleValue is StandardModuleTable)
                return new ConsoleFormat(stdModuleNameColor);

            return default;
        }

        static int GetSmartIndentationLevel(string text, int caret)
        {
            int openBraces = 0;
            var end = Math.Min(text.Length, caret);
            for (int i = 0; i < end; i++)
            {
                var c = text[i];
                if (c is '{' or '[' or '(')
                    ++openBraces;
                if (c is '}' or ']' or ')')
                    --openBraces;
            }
            return openBraces;
        }
    }
}
