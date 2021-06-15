using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IntSharp.Types;
using NUnit.Framework;

namespace IntervalEval.Tests
{
    public class Tests2
    {
        [Test]
        public void TestMutualInfo3()
        {
            var problemMutualInfo3 = ProblemDescriptor.Problems3D[1];
            var _rho1 = new Tuple<double, double>(1.0/9.0, 8.0/9.0);
            var _rho2 = new Tuple<double, double>(0.5, 0.5);
            var listInputMi = new List<object>() {_rho1, _rho2};
            var fMin = 0.0;
            var fMax = 0.0;
            var fPrec = 0.0;
            Optimizer.PrecisionF.OnChange += (sender, d) => fPrec = d;
            Optimizer.IntervalFMinimum.OnChange += (sender, d) => fMin = d;
            Optimizer.IntervalFMaximum.OnChange += (sender, d) => fMax = d;
            Optimizer.EvolutionBoxesAmount.OnChange += (sender, list) =>
            {
                Console.WriteLine($"Boxes amount : {list.Count}");
            };
            var sw = new Stopwatch();
            sw.Start();
            var result = Optimizer.Optimize(new[]
            {
                Interval.FromInfSup(-2, 2),
                Interval.FromInfSup(-2, 2),
                Interval.FromInfSup(-2, 2),
            }, problemMutualInfo3.Function, problemMutualInfo3.Constraints, OptimizationType.Maximization, 50, false, listInputMi);
            sw.Stop();
            Console.WriteLine($"Optimization took {sw.ElapsedMilliseconds} ms");
            // foreach (var optimizerSolution in result)
            // {
            //     IntervalHelpers.Print(optimizerSolution.Solutions);
            // }
            Console.WriteLine($"{fMin} , {fMax}, {fPrec}");
        }
    }
}