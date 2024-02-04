﻿using Nua.Types;

namespace Nua.CompileService.Syntaxes
{
    public class ForOfExpr : ForExpr
    {
        public ForOfExpr(string valueName, Expr start, Expr end, Expr? step, MultiExpr body)
        {
            ValueName = valueName;
            Start = start;
            End = end;
            Step = step;
            Body = body;
        }

        public string ValueName { get; }
        public Expr Start { get; }
        public Expr End { get; }
        public Expr? Step { get; }
        public MultiExpr Body { get; }

        public override NuaValue? Evaluate(NuaContext context, out EvalState state)
        {
            var start = Start.Evaluate(context);

            if (start == null)
                throw new NuaEvalException("Start value of 'for-of' statement is null");
            if (start is not NuaNumber startNumber)
                throw new NuaEvalException("Start value of 'for-of' statement not number");

            var end = End.Evaluate(context);

            if (end == null)
                throw new NuaEvalException("End value of 'for-of' statement is null");
            if (end is not NuaNumber endNumber)
                throw new NuaEvalException("End value of 'for-of' statement not number");

            NuaNumber? stepNumber = null;
            var step = Step?.Evaluate(context) as NuaNumber;

            if (Step != null && stepNumber == null)
                throw new NuaEvalException("Step value of 'for-of' statement not number");

            double startValue = startNumber.Value;
            double endValue = endNumber.Value;
            double stepValue = stepNumber?.Value ?? 1;

            NuaValue? result = null;
            if (endValue >= startValue)
            {
                stepValue = Math.Abs(stepValue);
                for (double value = startValue; value <= endValue; value += stepValue)
                {
                    context.Set(ValueName, new NuaNumber(value));

                    result = Body.Evaluate(context, out var bodyState);

                    if (bodyState == EvalState.Continue)
                        continue;
                    else if (bodyState == EvalState.Break)
                        break;
                }
            }
            else
            {
                stepValue = -Math.Abs(stepValue);
                for (double value = startValue; value >= endValue; value += stepValue)
                {
                    context.Set(ValueName, new NuaNumber(value));

                    result = Body.Evaluate(context, out var bodyState);

                    if (bodyState == EvalState.Continue)
                        continue;
                    else if (bodyState == EvalState.Break)
                        break;
                }
            }

            state = EvalState.None;
            return result;
        }
    }
}
