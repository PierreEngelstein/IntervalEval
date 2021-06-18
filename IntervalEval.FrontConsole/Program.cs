using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IntervalEval.Core;
using IntervalEval.Core.Helpers;
using IntervalEval.Core.Optimize;
using MathNet.Numerics.Optimization;

namespace IntervalEval.FrontConsole
{
    class Program
    {
        class Box
        {
            public Box(List<Interval> intervals)
            {
                Values = intervals;
            }

            public List<Interval> Values { get; set; }
            public Interval this[int index] => Values[index-1];
        }
        // static bool Disjoint(Interval a, Interval b)
        // {   
        //     return (a.Supremum<b.Infimum)||(b.Supremum<a.Infimum);
        // }

        // static bool Subset(Interval a, Interval b)
        // {
        //     return (b.Infimum<=a.Infimum)&&(a.Supremum<=b.Supremum);
        // }

        // static IntervalBoolean In(Interval F, Interval Y)
        // {
        //     if (F.Disjoint(Y)) return IntervalBoolean.False;
        //     return F.Subset(Y) ? IntervalBoolean.True : IntervalBoolean.Indeterminate;
        // }
        static IntervalBoolean Inside(Box P)
        {
            var Y = Interval.FromInfSup(-100000, 0);
            var positive = Interval.FromInfSup(0, 100000);
            var height = 2.3;
            IntervalBoolean inside1, inside2;
            Interval Ym1, Ym2, Ym3;
            Ym1 = 2 * IntervalMath.Sin(P[1]) + 1.5 * IntervalMath.Sin(P[1] + P[2]) - height;
            inside1 = Ym1.In(Y);
            Ym2 = 2 * IntervalMath.Sin(P[1]) - height;
            inside2 = Ym2.In(Y);
            // Ym3 = 2 * Math.Sin(P[3]) - height;
            return inside1 && inside2 && P[1].In(positive);
        }
        static int Optimize(Box P, double epsilon = 0.1)
        {
            var k = 0;
            var L_cur = new Stack<Box>();
            var L_sivia_IN = new Stack<Box>();
            var L_sivia_OUT = new Stack<Box>();
            var L_sivia_MB = new Stack<Box>();
            L_cur.Clear();
            L_cur.Push(P);
            var yellowCount = 0;
            while (L_cur.Any())
            {
                Console.WriteLine($"New iteration: k = {k}");
                P = L_cur.Pop();
                var test = Inside(P);
                if (P.Values.MaxWidth() < epsilon)
                {
                    if(test == IntervalBoolean.True) 
                        L_sivia_IN.Push(P);
                    else if(test == IntervalBoolean.False) 
                        L_sivia_OUT.Push(P);
                    else {
                        L_sivia_MB.Push(P);
                        k++;
                    }
                }else if (test == IntervalBoolean.True)
                {
                    L_sivia_IN.Push(P);
                    k++;
                }else if (test == IntervalBoolean.False)
                {
                    L_sivia_OUT.Push(P);
                    k++;
                }
                else
                {
                    var (item1, item2) = P.Values.Bisect(0.49);
                    L_cur.Push(new Box(item1.ToList()));
                    L_cur.Push(new Box(item2.ToList()));
                }
            }
            return k;
        }

        private static int failedBoxes = 0;
        static Interval MutualInfo(Interval a, Interval b)
        {
            var it01 = Interval.FromInfSup(0, 1);
            var p11 = 0.1*a;
            var p12 = 0.1*(1-a);
            var p21 =  0.9*b + 0.45*a + 0.09;
            var p22 = -0.9*b - 0.45*a + 0.81;
            var p11p21 = 0.9*b + 0.55*a + 0.09;
            var p12p22 = -0.9*b - 0.55*a + 0.91;
            var p11p12 = 0.1;
            var p21p22 = 0.9;
            try
            {
                p11 = p11.Intersection(it01);
                p12 = p12.Intersection(it01);
                p21 = p21.Intersection(it01);
                p22 = p22.Intersection(it01);
                p11p21 = p11p21.Intersection(it01);
                p12p22 = p12p22.Intersection(it01);
            }
            catch (Exception e)
            {
                Console.WriteLine("Disjoint !");
                return Interval.Zero;
            }
            
            var prev = IntervalMath.AmountFailXLog;
            var result =
                -IntervalMath.XLog(Interval.FromInfSup(p11p12, p11p12), "EX - H(P11 + P12)")
                -IntervalMath.XLog(Interval.FromInfSup(p21p22, p21p22), "EX - H(P21 + P22)")
                -IntervalMath.XLog(p11p21, "EY - H(P11+P21)")
                -IntervalMath.XLog(p12p22, "EY - H(P12 + P22)")
                +IntervalMath.XLog(p11, "EC - H(P11)")
                +IntervalMath.XLog(p12, "EC - H(P12)")
                +IntervalMath.XLog(p21, "EC - H(P21)")
                +IntervalMath.XLog(p22, "EC - H(P22)");
            var now = IntervalMath.AmountFailXLog;
            if (now != prev)
            {
                Console.Write("a = ");
                IntervalHelpers.Print(a, false);;
                Console.Write(" b = ");
                IntervalHelpers.Print(b);
                Console.Write("P11 = ");
                IntervalHelpers.Print(p11, false);
                Console.Write(" P12 = ");
                IntervalHelpers.Print(p12);
                Console.Write("P21 = ");
                IntervalHelpers.Print(p21, false);
                Console.Write(" P22 = ");
                IntervalHelpers.Print(p22);
                failedBoxes++;
            }
            return result;
        }
        
        
        static Interval MutualInfo(double a, double b)
        {
            var p11 = 0.1*a;
            var p12 = 0.1*(1-a);
            var p21 =  0.9*b + 0.45*a + 0.09;
            var p22 = -0.9*b - 0.45*a + 0.81;
            Console.WriteLine($"a = {a} ; b = {b}");
            Console.WriteLine($"P11 = {p11} P12 = {p12}");
            Console.WriteLine($"P21 = {p21} P22 = {p22}");
            var prev = IntervalMath.AmountFailXLog;
            var result =
                -IntervalMath.XLog(Interval.FromInfSup(p11+p12, p11+p12), "EX - H(P11 + P12)")
                -IntervalMath.XLog(Interval.FromInfSup(p21 + p22,p21 + p22), "EX - H(P21 + P22)")
                -IntervalMath.XLog(Interval.FromInfSup(p11+p21,p11+p21), "EY - H(P11+P21)")
                -IntervalMath.XLog(Interval.FromInfSup(p12+p22,p12+p22), "EY - H(P12 + P22)")
                +IntervalMath.XLog(Interval.FromInfSup(p11,p11), "EC - H(P11)")
                +IntervalMath.XLog(Interval.FromInfSup(p12,p12), "EC - H(P12)")
                +IntervalMath.XLog(Interval.FromInfSup(p21,p21), "EC - H(P21)")
                +IntervalMath.XLog(Interval.FromInfSup(p22,p22), "EC - H(P22)");
            var now = IntervalMath.AmountFailXLog;
            if (now != prev)
            {
                failedBoxes++;
            }
            return result;
        }

        static void Main(string[] args)
        {
            var problem = ProblemDescriptor.Problems3D[1];
            var durations = new List<double>();
            var angles = new List<double>();
            var fMin = 0.0;
            var fMax = 0.0;
            var fPrecision = 0.0;

            for (var i = 10; i <= 360; i++)
            {
                var angle = i * System.Math.PI / 180.0;
                angles.Add(angle);
                Console.WriteLine($"angle: {i} degrees");
                var p1 = new Tuple<double, double>(1.0, 0.0);
                var p2 = new Tuple<double, double>(System.Math.Cos(angle) * System.Math.Cos(angle), System.Math.Sin(angle) * System.Math.Sin(angle));
                Console.WriteLine(p2);
                var listInputMi = new List<object>{p1, p2};

                Optimizer.PrecisionF.OnChange += (_, d) =>
                {
                    if (double.IsNaN(d))
                    {
                        Console.WriteLine("[WARNING] NaN precision !");
                        return;
                    }
                    Console.WriteLine($"precision = {fPrecision}");
                    Console.WriteLine($"angle: {i} degrees");
                    fPrecision = d;
                };
                // Optimizer.IntervalFMinimum.OnChange += (_, d) =>
                // {
                //     if(double.IsNaN(d)) return;
                //     fMin = d;
                //     Console.WriteLine($"fMin = {fMin}");
                // };
                // Optimizer.IntervalFMaximum.OnChange += (_, d) =>
                // {
                //     if(double.IsNaN(d)) return;
                //     fMax = d;
                //     Console.WriteLine($"fMax = {fMax}");
                // };
                Optimizer.EvolutionBoxesAmount.OnChange += (_, boxes) =>
                {
                    if (!boxes.Any()) return;
                    Console.WriteLine($"{boxes.Count - 1} => {boxes[^1]}");
                    var proc = Process.GetCurrentProcess();
                    var mem = proc.PrivateMemorySize64 / (1024.0*1024.0);
                    Console.WriteLine($"RAM usage: {mem} MB");
                    Console.WriteLine($"angle: {i} degrees");
                    Console.WriteLine("===================");
                };
                
                var sw = new Stopwatch();
                sw.Restart();
                var result = Optimizer.Optimize(new[]
                {
                    Interval.FromInfSup(-2, 2),
                    Interval.FromInfSup(-2, 2),
                    Interval.FromInfSup(-2, 2),
                }, problem.Function, problem.Constraints, OptimizationType.Maximization, 50, false, listInputMi);
                sw.Stop();
                Console.WriteLine($"Optimization took {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"f in [{Optimizer.IntervalFMinimum.Value}, {Optimizer.IntervalFMaximum.Value}]");
                durations.Add(sw.ElapsedMilliseconds);
            }

            Optimizer.OptimizationIterations.Value = 0;
            Optimizer.PrecisionF.Value = 0;
            Optimizer.IntervalFMaximum.Value = 0;
            Optimizer.IntervalFMinimum.Value = 0;
            
            
            foreach (var angle in angles)
            {
                Console.Write($"{angle}, ");
            }
            Console.WriteLine("\n==========\n");
            foreach (var duration in durations)
            {
                Console.Write($"{duration}, ");
            }
        }
        
        static void Main1(string[] args)
        {
            var it = IntervalMath.Sqr(Interval.FromInfSup(2, 3) - Interval.FromInfSup(-4, 5)) + Interval.FromInfSup(-3, 2);
            IntervalHelpers.Print(it);
            return;
            // Testing x*log(x) undefined sections in mutual information
            var a = Interval.FromInfSup(0, 0.5);
            var b = Interval.FromInfSup(0, 1);
            var aCut = a.Cut(30);
            var bCut = b.Cut(30);

            var max = double.NegativeInfinity;
            var min = double.PositiveInfinity;
            var amountBoxes = 0;
            foreach (var itA in aCut)
            {
                foreach (var itB in bCut)
                {
                    amountBoxes++;
                    var mutualInfo = MutualInfo(itA, itB);
                    if (mutualInfo.Supremum > max) max = mutualInfo.Supremum;
                    if (mutualInfo.Infimum < min) min = mutualInfo.Infimum;
                    // IntervalHelpers.Print(mutualInfo);
                }
            }
            Console.WriteLine($"min = {min} ; max = {max} ; boxes amount = {amountBoxes}; failedBoxes = {failedBoxes}");
            Console.WriteLine("============================================");
            var result1 = MutualInfo(0.5, 0.68);
            IntervalHelpers.Print(result1);
            return;
            
            // c++ algorithm from robot2d. Adapted to c# and added 3rd dimension to test speed increase
            var resultOptim = Optimize(new Box(new List<Interval>()
                    {
                        Interval.FromInfSup(-System.Math.PI, System.Math.PI),
                        Interval.FromInfSup(-System.Math.PI, System.Math.PI),
                        Interval.FromInfSup(-System.Math.PI, System.Math.PI),
                    }), 0.01);
            Console.WriteLine(resultOptim);
            return;
            
            // 3 quantum state optimization
            var problemMutualInfo3 = ProblemDescriptor.Problems3D[3];
            var rho1 = new Tuple<double, double>(1.0, 0.0);
            var rho2 = new Tuple<double, double>(0.0, 0.1);
            var rho3 = new Tuple<double, double>(0.0, 0.0);
            var listInputMi = new List<object>() {rho1, rho2, rho3};
            // var listInputMi = new List<object>() {rho1, rho2};
            var fMin = 0.0;
            var fMax = 0.0;
            var fPrecision = 0.0;
            Optimizer.PrecisionF.OnChange += (sender, d) => fPrecision = d;
            Optimizer.IntervalFMinimum.OnChange += (sender, d) =>
            {
                fMin = d;
                Console.WriteLine($"fMin = {fMin}");
            };
            Optimizer.IntervalFMaximum.OnChange += (sender, d) =>
            {
                fMax = d;
                Console.WriteLine($"fMax = {fMax}");
            };
            Optimizer.EvolutionBoxesAmount.OnChange += (sender, boxes) =>
            {
                for (var i = 0; i < boxes.Count; i++)
                {
                    Console.WriteLine($"{i} => {boxes[i]}");
                }
                Console.WriteLine("===================");
            };
            var sw = new Stopwatch();
            sw.Start();
            var result = Optimizer.Optimize(new[]
            {
                Interval.FromInfSup(-2, 2),
                Interval.FromInfSup(-2, 2),
                Interval.FromInfSup(-2, 2),
                Interval.FromInfSup(-2, 2),
                Interval.FromInfSup(-2, 2),
                Interval.FromInfSup(-2, 2)
                
                // Interval.FromInfSup(-2, 2),
                // Interval.FromInfSup(-2, 2),
                // Interval.FromInfSup(-2, 2),
                // Interval.FromInfSup(-2, 2),
                // Interval.FromInfSup(-2, 2),
                // Interval.FromInfSup(-2, 2)
            }, problemMutualInfo3.Function, problemMutualInfo3.Constraints, OptimizationType.Maximization, 90, false, listInputMi);
            sw.Stop();
            Console.WriteLine($"Optimization took {sw.ElapsedMilliseconds} ms");
            // foreach (var optimizerSolution in result)
            // {
            //     IntervalHelpers.Print(optimizerSolution.Solutions);
            // }
            Console.WriteLine($"{fMin} , {fMax}, {fPrecision}");
            var optimizerSolutions = result as OptimizerSolution[] ?? result.ToArray();
            double minX = optimizerSolutions[0][0].Infimum, minY = optimizerSolutions[0][1].Infimum, minZ = optimizerSolutions[0][2].Infimum, minX2 = optimizerSolutions[0][3].Infimum, minY2 = optimizerSolutions[0][4].Infimum, minZ2 = optimizerSolutions[0][5].Infimum;
            double maxX = optimizerSolutions[0][0].Supremum, maxY = optimizerSolutions[0][1].Supremum, maxZ = optimizerSolutions[0][2].Supremum, maxX2 = optimizerSolutions[0][3].Supremum, maxY2 = optimizerSolutions[0][4].Supremum, maxZ2 = optimizerSolutions[0][5].Supremum;
            foreach (var optimizerSolution in optimizerSolutions)
            {
                if (optimizerSolution[0].Infimum <= minX) minX = optimizerSolution[0].Infimum;
                if (optimizerSolution[1].Infimum <= minY) minY = optimizerSolution[1].Infimum;
                if (optimizerSolution[2].Infimum <= minZ) minZ = optimizerSolution[2].Infimum;
                if (optimizerSolution[3].Infimum <= minX2) minX2 = optimizerSolution[3].Infimum;
                if (optimizerSolution[4].Infimum <= minY2) minY2 = optimizerSolution[4].Infimum;
                if (optimizerSolution[5].Infimum <= minZ2) minZ2 = optimizerSolution[5].Infimum;
                if (optimizerSolution[0].Supremum >= maxX) maxX = optimizerSolution[0].Supremum;
                if (optimizerSolution[1].Supremum >= maxY) maxY = optimizerSolution[1].Supremum;
                if (optimizerSolution[2].Supremum >= maxZ) maxZ = optimizerSolution[2].Supremum;                
                if (optimizerSolution[3].Supremum >= maxX2) maxX2 = optimizerSolution[3].Supremum;
                if (optimizerSolution[4].Supremum >= maxY2) maxY2 = optimizerSolution[4].Supremum;
                if (optimizerSolution[5].Supremum >= maxZ2) maxZ2 = optimizerSolution[5].Supremum;
            }
            Console.WriteLine($"Solution in [<{minX}, {maxX}>, <{minY}, {maxY}>, <{minZ}, {maxZ}>] [<{minX2}, {maxX2}>, <{minY2}, {maxY2}>, <{minZ2}, {maxZ2}>]");
        }
    }
}