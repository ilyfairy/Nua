﻿using System.Diagnostics.CodeAnalysis;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ChainExpr : Syntax
    {
        public ChainExpr(IEnumerable<Expr> expressions)
        {
            Expressions = expressions
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<Expr> Expressions { get; }

        public override NuaValue? Evaluate(NuaContext context)
        {
            NuaValue? value = null;
            foreach (var expr in Expressions)
                value = expr?.Evaluate(context);

            return value;
        }

        public override CompiledSyntax Compile()
        {
            List<CompiledSyntax> compiledSyntaxes = new(Expressions.Count);
            foreach (var expr in Expressions)
                compiledSyntaxes.Add(expr.Compile());

            return CompiledSyntax.CreateFromDelegate((context) =>
            {
                NuaValue? result = null;
                foreach (var compiledSyntax in compiledSyntaxes)
                    result = compiledSyntax.Evaluate(context);

                return result;
            });
        }

        public static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out ChainExpr? expr)
        {
            parseStatus = new();
            expr = null;
            int cursor = index;

            List<Expr> expressions = new();

            if (!Expr.Match(tokens, required, ref cursor, out parseStatus, out var firstExpr))
                return false;

            expressions.Add(firstExpr);

            while (TokenMatch(tokens, false, TokenKind.OptComma, ref cursor, out _, out _))
            {
                if (!Expr.Match(tokens, true, ref cursor, out parseStatus, out var nextExpr))
                {
                    if (parseStatus.Message == null)
                        parseStatus.Message = "Expect expression after ',' while parsing 'chain-expression'";

                    return false;
                }

                expressions.Add(nextExpr);
            }

            index = cursor;
            expr = new ChainExpr(expressions);
            return true;
        }

        public override IEnumerable<Syntax> TreeEnumerate()
        {
            foreach (var syntax in base.TreeEnumerate())
                yield return syntax;

            foreach (var expr in Expressions)
                foreach (var syntax in expr.TreeEnumerate())
                    yield return syntax;
        }
    }
}
