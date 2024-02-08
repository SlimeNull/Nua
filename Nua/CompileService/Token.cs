﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nua.CompileService;

public record struct Token(Range TextRange, TokenKind Kind, string? Value, int Ln, int Col);
