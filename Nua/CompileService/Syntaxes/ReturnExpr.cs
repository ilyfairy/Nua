﻿using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ReturnExpr : ProcessExpr
    {
        public Expr? ValueExpr { get; }

        public ReturnExpr(Expr? valueExpr)
        {
            ValueExpr = valueExpr;
        }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            state = EvalState.Return;
            return ValueExpr?.Evaluate(context);
        }

        public override CompiledProcessSyntax Compile()
        {
            return CompiledProcessSyntax.Create(ValueExpr?.Compile(), EvalState.Return);
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdReturn, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Message = null;
                return false;
            }

            Expr? value = null;
            if (TokenMatch(tokens, false, TokenKind.OptColon, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                if (!Expr.Match(tokens, true, ref cursor, out parseStatus, out value))
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
    }
}
