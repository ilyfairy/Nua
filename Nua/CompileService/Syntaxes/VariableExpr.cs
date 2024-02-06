﻿using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class VariableExpr : ValueExpr
    {
        public string Name { get; }
        public Token? NameToken { get; }

        public VariableExpr(string name)
        {
            Name = name;
        }

        public VariableExpr(Token nameToken)
        {
            if (nameToken.Value is null)
                throw new ArgumentException("Value of name token is null", nameof(nameToken));

            Name = nameToken.Value;
            NameToken = nameToken;
        }


        public override NuaValue? Evaluate(NuaContext context) => context.Get(Name);
        public override CompiledSyntax Compile() => CompiledSyntax.CreateFromDelegate(context => context.Get(Name));

        public void SetValue(NuaContext context, NuaValue? newValue)
        {
            context.Set(Name, newValue);
        }

        public new static bool Match(IList<Token> tokens, bool required, ref int index, out ParseStatus parseStatus, [NotNullWhen(true)] out Expr? expr)
        {
            parseStatus = new();
            expr = null;
            parseStatus.RequireMoreTokens = required;
            parseStatus.Message = null;


            if (!TokenMatch(tokens, required, TokenKind.Identifier, ref index, out _, out var idToken))
                return false;

            expr = new VariableExpr(idToken);
            return true;
        }
    }
}
