﻿using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{

    public class OrTailExpr : Expr
    {
        public OrTailExpr(Expr right, OrTailExpr? nextTail)
        {
            Right = right;
            NextTail = nextTail;
        }

        public Expr Right { get; }
        public OrTailExpr? NextTail { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            var rightValue = Right.Evaluate(context);

            if (rightValue == null)
            {
                if (NextTail == null)
                    return new NuaBoolean(false);
                else
                    return NextTail.Evaluate(context);
            }

            if (rightValue is not NuaBoolean rightBoolean)
                return rightValue;

            if (!rightBoolean.Value)
            {
                if (NextTail == null)
                    return new NuaBoolean(false);
                else
                    return NextTail.Evaluate(context);
            }

            return new NuaBoolean(true);
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out OrTailExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            if (!TokenMatch(tokens, required, TokenKind.KwdOr, ref cursor, out parseStatus.RequireMoreTokens, out _))
            {
                parseStatus.Intercept = required;
                parseStatus.Message = null;
                return false;
            }

            if (!AndExpr.Match(tokens, true, ref cursor, out parseStatus, out var right))
            {
                parseStatus.Intercept = true;
                if (parseStatus.Message == null)
                    parseStatus.Message = "Expect 'and-expression' after 'or' keyword";

                return false;
            }

            if (!Match(tokens, false, ref cursor, out var tailParseStatus, out var nextTail) && tailParseStatus.Intercept)
            {
                parseStatus = tailParseStatus;
                return false;
            }

            index = cursor;
            expr = new OrTailExpr(right, nextTail);
            parseStatus.RequireMoreTokens = false;
            parseStatus.Message = null;
            return true;
        }
    }
}
