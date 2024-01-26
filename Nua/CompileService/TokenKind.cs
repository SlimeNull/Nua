namespace Nua.CompileService
{
    public enum TokenKind
    {
        None,

        KwdRequire,

        KwdIf,
        KwdElse,
        KwdElif,

        KwdFor,
        KwdIn,
        KwdOf,
        KwdLoop,
        KwdWhile,
        KwdContinue,
        KwdBreak,

        KwdNull,
        KwdTrue,
        KwdFalse,

        KwdNot,
        KwdAnd,
        KwdOr,

        KwdFunction,
        KwdReturn,

        OptColon,  // :
        OptComma,  // ,
        OptDot,    // .

        OptAdd, // 加
        OptMin, // 减
        OptMul, // 乘
        OptDiv, // 除
        OptMod, // 模

        OptAddWith,  // +=
        OptMinWith,  // -=
        OptPow,      // 幂
        OptDivInt,   // 整除

        OptDoubleAdd,  // 自增
        OptDoubleMin,  // 自减

        OptEql,    // == 等于
        OptNeq,    // != 不等于
        OptLss,    // <  小于
        OptLeq,    // <= 小于等于
        OptGtr,    // >  大于
        OptGeq,    // >= 大于等于

        OptAssign,  // =  赋值

        ParenthesesLeft, ParenthesesRight,          // ( )
        SquareBracketLeft, SquareBracketRight,      // [ ]
        BigBracketLeft, BigBracketRight,            // { }

        Identifier,
        String,
        Number,
    }
}
