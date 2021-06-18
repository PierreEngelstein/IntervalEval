using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IntervalEval.Core.Optimize;

namespace IntervalEval.Core.Helpers
{
    public static class IntervalHelpers
    {
        /// <summary>
        /// Bisects the OptimizerSolution in the middle of the largest width
        /// </summary>
        /// <param name="input">Input OptimizerSolution to be bisected</param>
        /// <returns>Left and right OptimizerSolutions</returns>
        public static Tuple<OptimizerSolution, OptimizerSolution> Bisect(this OptimizerSolution input)
        {
            var (left, right) = input.Solutions.Bisect();
            return new Tuple<OptimizerSolution, OptimizerSolution>(
                new OptimizerSolution(left, input.GradientTagged, input.GradientSolution, input.Gradient),
                new OptimizerSolution(right, input.GradientTagged, input.GradientSolution, input.Gradient));
        }
        
        public static double Volume(this OptimizerSolution input)
        {
            return input.Solutions.Aggregate(1.0, (current, inputSolution) => current * (inputSolution.Supremum - inputSolution.Infimum));
        }

        /// <summary>
        /// Gets the middle point of an OptimizerSolution
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static OptimizerSolution Middle(this OptimizerSolution input)
        {
            return new(input.Solutions.Middle(), input.GradientTagged, input.GradientSolution, input.Gradient);
        }

        public static double MaxWidth(this IEnumerable<OptimizerSolution> solution)
        {
            return solution.Select(d => d.Solutions).MaxWidth();
        }
        
        public static void Print(IEnumerable<Interval> its)
        {
            Console.Write("[");
            foreach (var interval in its)
            {
                Console.Write($"[{interval.Infimum}, {interval.Supremum}]");
            }
            Console.WriteLine("]");
        }
        
        public static void Print(Interval it, bool newLine = true)
        {
            if(!newLine) 
                Console.Write($"[{it.Infimum}, {it.Supremum}]");
            else
                Console.WriteLine($"[{it.Infimum}, {it.Supremum}]");
        }

    }
}