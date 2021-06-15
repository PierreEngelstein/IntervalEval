using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntSharp.Types;
using IntSharp;
using Math = System.Math;

namespace IntervalEval
{
    public static class IntervalHelpers
    {
        private static readonly double XLogMin = System.Math.Exp(-1);
        public static int AmountFailXLog = 0;
        private const double ZeroPrecision = 1e-05;

        /** Custom method to evaluate f(x) = x*log(x) with f(0) = 0 instead of f(0) = -oo */
        public static Interval XLog(Interval it, string whereAmI = "")
        {
            // x \in [-oo, 0] => no result
            if (it.Supremum < 0)
            {
                // Console.WriteLine($"Encoutered undefined xlog at {whereAmI}");
                AmountFailXLog++;
                return Interval.Zero;
                throw new IntervalEvalException();
            }

            // if (it.Infimum < 0)
            // {
            //     Console.WriteLine($"Encoutered undefined xlog at {whereAmI}");
            //     AmountFailXLog++;
            //     return Interval.Zero;
            //     // throw new IntervalEvalException();
            // }
            // x \in [0+, exp(-1)] => y strictly decreasing
            if (it.Supremum <= XLogMin && it.Infimum > ZeroPrecision)
            {
                return Interval.FromInfSup(it.Supremum * System.Math.Log(it.Supremum), it.Infimum * System.Math.Log(it.Infimum));
            }
            // x \in [0-, exp(-1)] => y strictly decreasing and max(y) = 0
            if (it.Infimum <= ZeroPrecision && it.Supremum <= XLogMin)
            {
                return Interval.FromInfSup(it.Supremum * System.Math.Log(it.Supremum), 0);
            }
            // x \in [0+, +oo], x <= exp(-1)  => y increases then decreases
            if (it.Infimum <= XLogMin && it.Supremum >= XLogMin && it.Infimum >= ZeroPrecision)
            {
                return Interval.FromInfSup(XLogMin * System.Math.Log(XLogMin),
                    System.Math.Max(it.Infimum * System.Math.Log(it.Infimum), it.Supremum * System.Math.Log(it.Supremum)));
            }
            // x \in [0-, +00], x <= exp(-1) => y increases then decreases
            if (it.Infimum <= XLogMin && it.Supremum >= XLogMin && it.Infimum <= ZeroPrecision)
            {
                // if (System.Math.Max(0.0, it.Supremum * System.Math.Log(it.Supremum)) == 0.0)
                // {
                //     Console.WriteLine($"Encoutered undefined xlog at {whereAmI}");
                //     AmountFailXLog++;
                //     // return Interval.Zero;
                //     throw new IntervalEvalException();
                // }
                return Interval.FromInfSup(XLogMin * System.Math.Log(XLogMin),
                    System.Math.Max(0.0, it.Supremum * System.Math.Log(it.Supremum)));
            }
            // x \in [exp(-1), +oo] => y strictly increasing
            if (it.Infimum >= XLogMin)
            {
                return Interval.FromInfSup(it.Infimum * System.Math.Log(it.Infimum), it.Supremum * System.Math.Log(it.Supremum)); 
            }
            // No solution otherwise
            // Console.WriteLine($"Encoutered undefined xlog at {whereAmI}");
            AmountFailXLog++;
            return Interval.Zero;
            throw new IntervalEvalException();
        }

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
        
        public static Tuple<IEnumerable<Interval>, IEnumerable<Interval>> Bisect(this IEnumerable<Interval> input)
        {
            var enumerable = input as List<Interval> ?? input.ToList();
            var largestWidth = enumerable[0].Diam();
            var largest = enumerable[0];
            foreach (var interval in enumerable.Where(interval => interval.Diam() >= largestWidth))
            {
                largestWidth = interval.Diam();
                largest = interval;
            }
            var (left, right) = largest.Bisect();
            var bisectIndex = enumerable.IndexOf(largest);
            var rightBox = enumerable.ToList();
            var leftBox = enumerable.ToList();
            rightBox[bisectIndex] = right;
            leftBox[bisectIndex] = left;
            return new Tuple<IEnumerable<Interval>, IEnumerable<Interval>>(leftBox, rightBox);
        }
        
        public static Tuple<IEnumerable<Interval>, IEnumerable<Interval>> Bisect(this IEnumerable<Interval> input, double percent)
        {
            var enumerable = input as List<Interval> ?? input.ToList();
            var largestWidth = enumerable[0].Diam();
            var largest = enumerable[0];
            foreach (var interval in enumerable.Where(interval => interval.Diam() >= largestWidth))
            {
                largestWidth = interval.Diam();
                largest = interval;
            }
            var (left, right) = largest.Bisect(percent);
            var bisectIndex = enumerable.IndexOf(largest);
            var rightBox = enumerable.ToList();
            var leftBox = enumerable.ToList();
            rightBox[bisectIndex] = right;
            leftBox[bisectIndex] = left;
            return new Tuple<IEnumerable<Interval>, IEnumerable<Interval>>(leftBox, rightBox);
        }

        public static OptimizerSolution Middle(this OptimizerSolution input)
        {
            return new OptimizerSolution(input.Solutions.Middle(), input.GradientTagged, input.GradientSolution, input.Gradient);
        }

        public static IEnumerable<Interval> Middle(this IEnumerable<Interval> input)
        {
            return input.Select(interval => Interval.FromInfSup(interval.Mid(), interval.Mid())).ToList();
        }

        private static Interval Trace(Interval x11, Interval x12, Interval x22, Interval y11, Interval y12, Interval y22)
        {
            return x11 * y11 + 2 * x12 * y12 + x22 * y22;
        }

        private static Interval EntropyM(Interval x1_11, Interval x1_12, Interval x1_22, Interval x2_11, Interval x2_12,
            Interval x2_22, Interval y1_11, Interval y1_12, Interval y1_22, Interval y2_11, Interval y2_12,
            Interval y2_22)
        {
            return -XLog(Trace(x1_11, x1_12, x1_22, y1_11, y1_12, y1_22) + Trace(x1_11, x1_12, x1_22, y2_11, y2_12, y2_22), "EntropyM") -
                   XLog(Trace(x2_11, x2_12, x2_22, y1_11, y1_12, y1_22) + Trace(x2_11, x2_12, x2_22, y2_11, y2_12, y2_22), "EntrpyM");
        }

        private static Interval EntropyMP(Interval x1_11, Interval x1_12, Interval x1_22, Interval x2_11, Interval x2_12,
            Interval x2_22, Interval y1_11, Interval y1_12, Interval y1_22, Interval y2_11, Interval y2_12,
            Interval y2_22)
        {
            return -XLog(Trace(x1_11, x1_12, x1_22, y1_11, y1_12, y1_22), "Joint Entropy") -
                   XLog(Trace(x1_11, x1_12, x1_22, y2_11, y2_12, y2_22), "Joint Entropy") -
                   XLog(Trace(x2_11, x2_12, x2_22, y1_11, y1_12, y1_22), "Joint Entropy") -
                   XLog(Trace(x2_11, x2_12, x2_22, y2_11, y2_12, y2_22), "Joint Entropy");
        }

        public static Interval MutualInformation(Interval a, Interval b, Interval d, Interval x2_11,
            Interval x2_12, Interval x2_22, Interval y1_11, Interval y1_12, Interval y1_22, Interval y2_11, Interval y2_12,
            Interval y2_22)
        {
            return EntropyM(a, b, d, x2_11, x2_12, x2_22, y1_11, y1_12, y1_22, y2_11, y2_12, y2_22)
                + (-0.1 * Math.Log(0.1) - 0.9*Math.Log(0.9)) 
                - EntropyMP(a, b, d, x2_11, x2_12, x2_22, y1_11, y1_12, y1_22, y2_11, y2_12, y2_22);
        }
        
        public static Interval MutualInformation(Interval a, Interval b, Interval d, Interval x2_11,
            Interval x2_12, Interval x2_22, double y1_11, double y1_12, double y1_22, double y2_11, double y2_12,
            double y2_22)
        {
            
            return EntropyM(a, b, d, x2_11, x2_12, x2_22, 
                       Interval.FromInfSup( y1_11, y1_11), Interval.FromInfSup(y1_12, y1_12), Interval.FromInfSup(y1_22, y1_22), 
                       Interval.FromInfSup(y2_11, y2_11), Interval.FromInfSup(y2_12, y2_12), Interval.FromInfSup(y2_22, y2_22))
                   + (-0.1 * Math.Log(0.1) - 0.9*Math.Log(0.9)) 
                   - EntropyMP(a, b, d, x2_11, x2_12, x2_22, 
                       Interval.FromInfSup(y1_11, y1_11), Interval.FromInfSup(y1_12, y1_12), Interval.FromInfSup(y1_22, y1_22), 
                       Interval.FromInfSup(y2_11, y2_11), Interval.FromInfSup(y2_12, y2_12), Interval.FromInfSup(y2_22, y2_22));
        }
        
        public static Interval MutualInformationBis(Interval x1_11, Interval x1_12, Interval x1_22, Interval x2_11,
            Interval x2_12, Interval x2_22, Interval y1_11, Interval y1_12, Interval y1_22, Interval y2_11, Interval y2_12,
            Interval y2_22)
        {
            return EntropyM(x1_11, x1_12, x1_22, x2_11, x2_12, x2_22, y1_11, y1_12, y1_22, y2_11, y2_12, y2_22)
                   + (-0.1 * Math.Log(0.1) - 0.9*Math.Log(0.9)) 
                   - EntropyMP(x1_11, x1_12, x1_22, x2_11, x2_12, x2_22, y1_11, y1_12, y1_22, y2_11, y2_12, y2_22);
        }

        // Computes mutual information from Interval input and double output
        public static Tuple<Interval, bool, int, IEnumerable<Interval>> MutualInformation(Interval a, Interval b, Interval d, double y1A, double y1B, double y1D, double y2A, double y2B, double y2D, bool isGradientToUse, int valSolGradient, IEnumerable<Interval> previousGradient)
        {
            // var maxMutualInfo = double.NegativeInfinity;
            // for (var i = 0; i < Math.Pow(2, 3); i++)
            // {
            //     var bitArray1 = new BitArray(new[] {i});
            //     var valA = bitArray1[0] ? a.Infimum : a.Supremum;
            //     var valB = bitArray1[1] ? b.Infimum : b.Supremum;
            //     var valD = bitArray1[2] ? d.Infimum : d.Supremum;
            //     var mutualInfo = MutualInformation(valA, valB, valD, y1A, y1B, y1D, y2A, y2B, y2D);
            //     if (mutualInfo > maxMutualInfo) maxMutualInfo = mutualInfo;
            // }
            // // Console.WriteLine($"Maximum is {maxMutualInfo}");
            // // Console.WriteLine("==========");
            //
            // var optimistMutualInfo = MutualInformation(a, b, d,
            //     Interval.FromInfSup(y1A, y1A), Interval.FromInfSup(y1B, y1B), Interval.FromInfSup(y1D, y1D),
            //     Interval.FromInfSup(y2A, y2A), Interval.FromInfSup(y2B, y2B), Interval.FromInfSup(y2D, y2D));
            //
            // var betterMutualInfo = Interval.FromInfSup(optimistMutualInfo.Infimum, maxMutualInfo);
            // return new Tuple<Interval, bool, int, IEnumerable<Interval>>(betterMutualInfo, false, -1, null);

            // return new Tuple<Interval, bool, int, IEnumerable<Interval>>(optimistMutualInfo, false, -1,
            //     previousGradient);
            
            
            // var valueSolutionGradient = -1;
            // if (!isGradientToUse)
            // {
                // // Console.WriteLine("CAN USE GRADIENT");
                // var gradient = GradientMutualInfo(a, b, d, y1A, y1B, y1D, y2A, y2B, y2D).ToList();
                // // Console.WriteLine("Gradient = ");
                // // Console.Write("    da = ");
                // // Print(gradient[0]);
                // // Console.Write("    db = ");
                // // Print(gradient[1]);
                // // Console.Write("    dd = ");
                // // Print(gradient[2]);
                // if (gradient[0].Infimum > 0 && gradient[1].Infimum > 0 && gradient[2].Infimum > 0)
                // {
                //     valueSolutionGradient = 0;
                // }
                // else if (gradient[0].Supremum < 0 && gradient[1].Infimum > 0 && gradient[2].Infimum > 0)
                // {
                //     valueSolutionGradient = 4;
                // }
                //
                // else if (gradient[0].Infimum > 0 && gradient[1].Supremum < 0 && gradient[2].Infimum > 0)
                // {
                //     valueSolutionGradient = 2;
                // }
                //
                // else if (gradient[0].Infimum > 0 && gradient[1].Infimum > 0 && gradient[2].Supremum < 0)
                // {
                //     valueSolutionGradient = 1;
                // }
                //
                // else if (gradient[0].Supremum < 0 && gradient[1].Supremum < 0 && gradient[2].Infimum > 0)
                // {
                //     valueSolutionGradient = 6;
                // }
                //
                // else if (gradient[0].Supremum < 0 && gradient[1].Infimum > 0 && gradient[2].Supremum > 0)
                // {
                //     valueSolutionGradient = 5;
                // }
                //
                // else if (gradient[0].Infimum > 0 && gradient[1].Supremum < 0 && gradient[2].Supremum < 0)
                // {
                //     valueSolutionGradient = 3;
                // }
                //
                // else if (gradient[0].Supremum < 0 && gradient[1].Supremum < 0 && gradient[2].Supremum < 0)
                // {
                //     valueSolutionGradient = 7;
                // }
                // else {

                
                // Compute interval using mean value form
                var grad = GradientMutualInfo(a, b, d, y1A, y1B, y1D, y2A, y2B, y2D).ToArray();
                var fX0 = MutualInformation(a.Infimum, b.Infimum, d.Infimum, y1A, y1B, y1D, y2A, y2B, y2D);
                var fMeanValueForm = fX0 + grad[0] * (a - a.Infimum) + grad[1] * (b - b.Infimum) + grad[2] * (d - d.Infimum);
                
                // Compute interval using natural inclusion function
                var fNaturalInclusion = MutualInformation(a, b, d,
                    Interval.FromInfSup(y1A, y1A), Interval.FromInfSup(y1B, y1B), Interval.FromInfSup(y1D, y1D),
                    Interval.FromInfSup(y2A, y2A), Interval.FromInfSup(y2B, y2B), Interval.FromInfSup(y2D, y2D));
                
                // Intersect both evaluations to get smallest evaluation possible
                var fIntersected = fMeanValueForm.Intersection(fNaturalInclusion);
                if (fIntersected.IsEmpty()) fIntersected = fNaturalInclusion;
                
                return new Tuple<Interval, bool, int, IEnumerable<Interval>>(fIntersected, false, -1, previousGradient);

                return new Tuple<Interval, bool, int, IEnumerable<Interval>>(MutualInformation(a, b, d,
                    Interval.FromInfSup(y1A, y1A), Interval.FromInfSup(y1B, y1B), Interval.FromInfSup(y1D, y1D),
                    Interval.FromInfSup(y2A, y2A), Interval.FromInfSup(y2B, y2B), Interval.FromInfSup(y2D, y2D)), false, -1, previousGradient);
                // }
            // }
            // if (valueSolutionGradient == -1) valueSolutionGradient = valSolGradient;
            //
            // var bitArray = new BitArray(new[] {valueSolutionGradient});
            // // Evaluate F as a point from inf and sup instead of interval
            // // [df]([x]) > 0 => f([x]) = [[f](x.inf), [f](x.sup)] is completely included in [f]([x])
            // return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(
            //     MutualInformation(bitArray[0] ? a.Supremum : a.Infimum, bitArray[1] ? b.Supremum : b.Infimum, bitArray[2] ? d.Supremum : d.Infimum, y1A, y1B, y1D, y2A, y2B, y2D),
            //     MutualInformation(bitArray[0] ? a.Infimum : a.Supremum, bitArray[1] ? b.Infimum : b.Supremum, bitArray[2] ? d.Infimum : d.Supremum, y1A, y1B, y1D, y2A, y2B, y2D)), true, valueSolutionGradient, previousGradient);
        }


        public static Tuple<Interval, bool, int, IEnumerable<Interval>> MutualInformation3Vars(Interval a1, Interval b1, Interval d1, Interval a2, Interval b2, Interval d2, double y1A,
            double y1B, double y1D, double y2A, double y2B, double y2D, double y3A, double y3B, double y3D, bool isGradientToUse, int valSolGradient, IEnumerable<Interval> previousGradient)
        {
            
            var valueSolutionGradient = -1;
            if (!isGradientToUse)
            {
                var gradient = GradientMutualInfo3Vars(a1, b1, d1, a2, b2, d2, y1A, y1B, y1D, y2A, y2B, y2D, y3A, y3B, y3D);
                var inputValues = new List<Interval> {a1, b1, d1, a2, b2, d2};
                var enumerable = gradient as Interval[] ?? gradient.ToArray();
                var ok = false;
                for (var i = 0; i < Math.Pow(2, enumerable.Length); i++)
                {
                    bool result = true;
                    var valuesTop = new List<double>();
                    var valuesBottom = new List<double>();
                    var bitArray = new BitArray(new[] {i});
                    for (var j = 0; j < enumerable.Length; j++)
                    {
                        if (bitArray[j])
                        {
                            result = result && (enumerable[j].Supremum < 0);
                            valuesTop.Add(inputValues[j].Supremum);
                            valuesBottom.Add(inputValues[j].Infimum);
                        }
                        else
                        {
                            result = result && (enumerable[j].Infimum > 0);
                            valuesTop.Add(inputValues[j].Infimum);
                            valuesBottom.Add(inputValues[j].Supremum);
                        }
                    }

                    if (result)
                    {
                        ok = true;
                        valueSolutionGradient = i;
                        break;
                    }
                }

                if (!ok)
                {
                    return new Tuple<Interval, bool, int, IEnumerable<Interval>>(
                        MutualInformation3Vars(a1, b1, d1, a2, b2, d2, y1A, y1B, y1D, y2A, y2B, y2D, y3A, y3B, y3D), 
                        false, -1, enumerable);
                }
            }
            if (valueSolutionGradient == -1) valueSolutionGradient = valSolGradient;
            var bitArrayLoc = new BitArray(new[] {valueSolutionGradient});
            return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(
                MutualInformation3Vars(
                    bitArrayLoc[0] ? a1.Supremum : a1.Infimum,
                    bitArrayLoc[1] ? b1.Supremum : b1.Infimum, 
                    bitArrayLoc[2] ? d1.Supremum : d1.Infimum,
                    bitArrayLoc[3] ? a2.Supremum : a2.Infimum, 
                    bitArrayLoc[4] ? b2.Supremum : b2.Infimum,
                    bitArrayLoc[5] ? d2.Supremum : d2.Infimum,
                    y1A, y1B, y1D, y2A, y2B, y2D, y3A, y3B, y3D),
                MutualInformation3Vars(
                    bitArrayLoc[0] ? a1.Infimum : a1.Supremum,
                    bitArrayLoc[1] ? b1.Infimum : b1.Supremum, 
                    bitArrayLoc[2] ? d1.Infimum : d1.Supremum,
                    bitArrayLoc[3] ? a2.Infimum : a2.Supremum, 
                    bitArrayLoc[4] ? b2.Infimum : b2.Supremum,
                    bitArrayLoc[5] ? d2.Infimum : d2.Supremum,
                    y1A, y1B, y1D, y2A, y2B, y2D, y3A, y3B, y3D)), true, valueSolutionGradient, previousGradient);

            // if (!isGradientToUse)
            // {
            //     var gradient = GradientMutualInfo3Vars(a1, b1, d1, a2, b2, d2, y1A, y1B, y1D, y2A, y2B, y2D, y3A, y3B, y3D);
            //     var inputValues = new List<Interval> {a1, b1, d1, a2, b1, d2};
            //     var enumerable = gradient as Interval[] ?? gradient.ToArray();
            //     for (var i = 0; i < Math.Pow(2, enumerable.Length); i++)
            //     {
            //         bool result = true;
            //         var valuesTop = new List<double>();
            //         var valuesBottom = new List<double>();
            //         var bitArray = new BitArray(new[] {i});
            //         for (var j = 0; j < enumerable.Length; j++)
            //         {
            //             if (bitArray[j])
            //             {
            //                 result = result && (enumerable[j].Supremum < 0);
            //                 valuesTop.Add(inputValues[j].Supremum);
            //                 valuesBottom.Add(inputValues[j].Infimum);
            //             }
            //             else
            //             {
            //                 result = result && (enumerable[j].Infimum > 0);
            //                 valuesTop.Add(inputValues[j].Infimum);
            //                 valuesBottom.Add(inputValues[j].Supremum);
            //             }
            //         }
            //
            //         if (result)
            //         {
            //             return new Tuple<Interval, bool>(Interval.FromInfSup(
            //                 MutualInformation3Vars(valuesTop[0], valuesTop[1], valuesTop[2], valuesTop[3], valuesTop[4],
            //                     valuesTop[5], y1A, y1B, y1D, y2A, y2B, y2D, y3A, y3B, y3D),
            //                 MutualInformation3Vars(valuesBottom[0], valuesBottom[1], valuesBottom[2], valuesBottom[3], valuesBottom[4],
            //                     valuesBottom[5], y1A, y1B, y1D, y2A, y2B, y2D, y3A, y3B, y3D)
            //             ), true);
            //         }
            //     }
            //     return new Tuple<Interval, bool>(MutualInformation3Vars(a1, b1, d1, a2, b2, d2, y1A, y1B, y1D, y2A, y2B, y2D, y3A, y3B, y3D), false);
            // }
            // Console.WriteLine("CANNOT USE GRADIENT: ALREADY / or \\");
            // return new Tuple<Interval, bool, int, IEnumerable<Interval>>(
            //     MutualInformation3Vars(a1, b1, d1, a2, b2, d2, y1A, y1B, y1D, y2A, y2B, y2D, y3A, y3B, y3D), 
            //     false, -1, previousGradient);
        }

        public static Interval MutualInformation3Vars(Interval a1, Interval b1, Interval d1, Interval a2,
            Interval b2, Interval d2, double y1A,
            double y1B, double y1D, double y2A, double y2B, double y2D, double y3A, double y3B, double y3D)
        {
            var result = 
                -XLog(d2*y3D+2*b2*y3B+a2*y3A+d2*y2D+2*b2*y2B+a2*y2A+d2*y1D+2*b2*y1B+a2*y1A, "EntropyM")
                -XLog(d1*y3D+2*b1*y3B+a1*y3A+d1*y2D+2*b1*y2B+a1*y2A+d1*y1D+2*b1*y1B+a1*y1A, "EntropyM")
                -XLog((-d2-d1+1)*y3D+2*(-b2-b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+2*(-b2-b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+2*(-b2-b1)*y1B+(-a2-a1+1)*y1A, "EntropyM")
                +XLog(d2*y3D+2*b2*y3B+a2*y3A, "Joint Entropy (32)")
                +XLog((-d2-d1+1)*y3D+2*(-b2-b1)*y3B+(-a2-a1+1)*y3A, "Joint Entropy (33)")
                +XLog(d1*y3D+2*b1*y3B+a1*y3A, "Joint Entropy (31)")
                +XLog(d2*y2D+2*b2*y2B+a2*y2A, "Joint Entropy (22)")
                +XLog((-d2-d1+1)*y2D+2*(-b2-b1)*y2B+(-a2-a1+1)*y2A, "Joint Entropy (23)")
                +XLog(d1*y2D+2*b1*y2B+a1*y2A, "Joint Entropy (21)")
                +XLog(d2*y1D+2*b2*y1B+a2*y1A, "Joint Entropy (12)")
                +XLog((-d2-d1+1)*y1D+2*(-b2-b1)*y1B+(-a2-a1+1)*y1A, "Joint Entropy (13)")
                +XLog(d1*y1D+2*b1*y1B+a1*y1A, "Joint Entropy (11)") + (-XLog(0.1) - XLog(0.6) - XLog(0.3));
            return result;
        }
        public static double MutualInformation3Vars(double a1, double b1, double d1, double a2, double b2, double d2, double y1A,
            double y1B, double y1D, double y2A, double y2B, double y2D, double y3A, double y3B, double y3D)
        {
            var result = 
                -XLog(d2*y3D+2*b2*y3B+a2*y3A+d2*y2D+2*b2*y2B+a2*y2A+d2*y1D+2*b2*y1B+a2*y1A)
                -XLog(d1*y3D+2*b1*y3B+a1*y3A+d1*y2D+2*b1*y2B+a1*y2A+d1*y1D+2*b1*y1B+a1*y1A)
                -XLog((-d2-d1+1)*y3D+2*(-b2-b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+2*(-b2-b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+2*(-b2-b1)*y1B+(-a2-a1+1)*y1A)
                +XLog(d2*y3D+2*b2*y3B+a2*y3A)
                +XLog((-d2-d1+1)*y3D+2*(-b2-b1)*y3B+(-a2-a1+1)*y3A)
                +XLog(d1*y3D+2*b1*y3B+a1*y3A)
                +XLog(d2*y2D+2*b2*y2B+a2*y2A)
                +XLog((-d2-d1+1)*y2D+2*(-b2-b1)*y2B+(-a2-a1+1)*y2A)
                +XLog(d1*y2D+2*b1*y2B+a1*y2A)
                +XLog(d2*y1D+2*b2*y1B+a2*y1A)
                +XLog((-d2-d1+1)*y1D+2*(-b2-b1)*y1B+(-a2-a1+1)*y1A)
                +XLog(d1*y1D+2*b1*y1B+a1*y1A) + (-XLog(0.1) - XLog(0.6) - XLog(0.3));;
            return result;
        }

        public static IEnumerable<Interval> GradientMutualInfo3Vars(Interval a1, Interval b1, Interval d1, Interval a2,
            Interval b2, Interval d2, double y1A,
            double y1B, double y1D, double y2A, double y2B, double y2D, double y3A, double y3B, double y3D)
        {
            var diffA1 = 
                (y3A+y2A+y1A)*IntSharp.Math.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                +(-y3A-y2A-y1A)*IntSharp.Math.Ln(d1*y3D+2*b1*y3B+a1*y3A+d1*y2D+2*b1*y2B+a1*y2A+d1*y1D+2*b1*y1B+a1*y1A)
                -y3A*IntSharp.Math.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A)
                +y3A*IntSharp.Math.Ln(d1*y3D+2*b1*y3B+a1*y3A)
                -y2A*IntSharp.Math.Ln((-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A)
                +y2A*IntSharp.Math.Ln(d1*y2D+2*b1*y2B+a1*y2A)
                -y1A*IntSharp.Math.Ln((-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                +y1A*IntSharp.Math.Ln(d1*y1D+2*b1*y1B+a1*y1A);
            var diffB1 =
                (2*y3B+2*y2B+2*y1B)*IntSharp.Math.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                -2*y3B*IntSharp.Math.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A)
                +(-2*y3B-2*y2B-2*y1B)*IntSharp.Math.Ln(d1*y3D+2*b1*y3B+a1*y3A+d1*y2D+2*b1*y2B+a1*y2A+d1*y1D+2*b1*y1B+a1*y1A)
                +2*y3B*IntSharp.Math.Ln(d1*y3D+2*b1*y3B+a1*y3A)
                -2*y2B*IntSharp.Math.Ln((-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A)
                +2*y2B*IntSharp.Math.Ln(d1*y2D+2*b1*y2B+a1*y2A)
                -2*y1B*IntSharp.Math.Ln((-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                +2*y1B*IntSharp.Math.Ln(d1*y1D+2*b1*y1B+a1*y1A);
            var diffD1 =
                (y3D+y2D+y1D)*IntSharp.Math.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                -y3D*IntSharp.Math.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A)
                +(-y3D-y2D-y1D)*IntSharp.Math.Ln(d1*y3D+2*b1*y3B+a1*y3A+d1*y2D+2*b1*y2B+a1*y2A+d1*y1D+2*b1*y1B+a1*y1A)
                +y3D*IntSharp.Math.Ln(d1*y3D+2*b1*y3B+a1*y3A)
                -y2D*IntSharp.Math.Ln((-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A)
                +y2D*IntSharp.Math.Ln(d1*y2D+2*b1*y2B+a1*y2A)
                -y1D*IntSharp.Math.Ln((-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                +y1D*IntSharp.Math.Ln(d1*y1D+2*b1*y1B+a1*y1A);
            var diffA2 = 
                (-y3A-y2A-y1A)*IntSharp.Math.Ln(d2*y3D+2*b2*y3B+a2*y3A+d2*y2D+2*b2*y2B+a2*y2A+d2*y1D+2*b2*y1B+a2*y1A)
                +y3A*IntSharp.Math.Ln(d2*y3D+2*b2*y3B+a2*y3A)
                +(y3A+y2A+y1A)*IntSharp.Math.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                -y3A*IntSharp.Math.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A)
                +y2A*IntSharp.Math.Ln(d2*y2D+2*b2*y2B+a2*y2A)
                -y2A*IntSharp.Math.Ln((-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A)
                +y1A*IntSharp.Math.Ln(d2*y1D+2*b2*y1B+a2*y1A)
                -y1A*IntSharp.Math.Ln((-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A);
            var diffB2 =
                (-2*y3B-2*y2B-2*y1B)*IntSharp.Math.Ln(d2*y3D+2*b2*y3B+a2*y3A+d2*y2D+2*b2*y2B+a2*y2A+d2*y1D+2*b2*y1B+a2*y1A)
                +2*y3B*IntSharp.Math.Ln(d2*y3D+2*b2*y3B+a2*y3A)
                +(2*y3B+2*y2B+2*y1B)*IntSharp.Math.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                -2*y3B*IntSharp.Math.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A)
                +2*y2B*IntSharp.Math.Ln(d2*y2D+2*b2*y2B+a2*y2A)
                -2*y2B*IntSharp.Math.Ln((-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A)
                +2*y1B*IntSharp.Math.Ln(d2*y1D+2*b2*y1B+a2*y1A)
                -2*y1B*IntSharp.Math.Ln((-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A);
            var diffD2 = 
                (-y3D-y2D-y1D)*IntSharp.Math.Ln(d2*y3D+2*b2*y3B+a2*y3A+d2*y2D+2*b2*y2B+a2*y2A+d2*y1D+2*b2*y1B+a2*y1A)
                +y3D*IntSharp.Math.Ln(d2*y3D+2*b2*y3B+a2*y3A)
                +(y3D+y2D+y1D)*IntSharp.Math.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                -y3D*IntSharp.Math.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A)
                +y2D*IntSharp.Math.Ln(d2*y2D+2*b2*y2B+a2*y2A)
                -y2D*IntSharp.Math.Ln((-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A)
                +y1D*IntSharp.Math.Ln(d2*y1D+2*b2*y1B+a2*y1A)
                -y1D*IntSharp.Math.Ln((-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A);
            return new List<Interval>{diffA1, diffB1, diffD1, diffA2, diffB2, diffD2};
        }

        /// <summary>
        /// Computes the mutual information of numbers.
        /// </summary>
        public static double MutualInformation(double a, double b, double d,
            double p1A, double p1B, double p1D, double p2A, double p2B, double p2D)
        {
            return 
                -XLog(p2D*d+p1D*d+2*p2B*b+2*p1B*b+p2A*a+p1A*a)
                -XLog(p2D*(1-d)+p1D*(1-d)-2*p2B*b-2*p1B*b+p2A*(1-a)+p1A*(1-a))
                +XLog(p2D*d+2*p2B*b+p2A*a)
                +XLog(p1D*d+2*p1B*b+p1A*a)
                +XLog(p2D*(1-d)-2*p2B*b+p2A*(1-a))
                +XLog(p1D*(1-d)-2*p1B*b+p1A*(1-a))
                +(-XLog(0.1) - XLog(0.9));
        }
        
        static Interval it01 = Interval.FromInfSup(0, 1);

        /// <summary>
        /// Computes the mutual information of intervals.
        /// </summary>
        public static Interval MutualInformation(Interval a, Interval b, Interval d,
            Interval P1_1, Interval P1_2, Interval P1_3, Interval P2_1, Interval P2_2, Interval P2_3)
        {
            var p11 = a*P1_1+2*b*P1_2+d*P1_3;
            var p12 = (1-a)*P1_1-2*b*P1_2+(1-d)*P1_3;
            var p21 = a*P2_1+2*b*P2_2+d*P2_3;
            var p22 = (1-a)*P2_1-2*b*P2_2+(1-d)*P2_3;
            var p11p21 = (P2_3+P1_3)*d+(2*P2_2+2*P1_2)*b+(P2_1+P1_1)*a;
            var p12p22 = (-P2_3-P1_3)*d+(-2*P2_2-2*P1_2)*b+(-P2_1-P1_1)*a+P2_3+P2_1+P1_3+P1_1;
            
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
                return Interval.Empty;
            }
            
            return 
                -XLog(p11p21, "Entropy p11+p12")
                -XLog(p12p22, "Entropy p12+p22")
                +XLog(p21,    "Joint Entropy (21)")
                +XLog(p11,    "Joint Entropy (11)")
                +XLog(p22,    "Joint Entropy (22)")
                +XLog(p12,    "Joint Entropy (12)")
                +(-XLog(0.1) - XLog(0.9));
        }
        
        /// <summary>
        /// Computes the gradient of the mutual information, obtained with wxMaxima.
        /// grad(MI) = (d(MI)/da , d(MI)/db , d(MI)/dd).
        /// </summary>
        public static IEnumerable<Interval> GradientMutualInfo(Interval a, Interval b, Interval d,
            double P1_1, double P1_2, double P1_3, double P2_1, double P2_2, double P2_3)
        {
            var diffGradA = 
                (-P2_1-P1_1)*IntSharp.Math.Ln((P2_3+P1_3)*d+(2*P2_2+2*P1_2)*b+(P2_1+P1_1)*a)
                +P2_1*IntSharp.Math.Ln(P2_3*d+2*P2_2*b+P2_1*a)
                -P2_1*IntSharp.Math.Ln(-P2_3*d-2*P2_2*b-P2_1*a+P2_3+P2_1)
                +(P2_1+P1_1)*IntSharp.Math.Ln((-P2_3-P1_3)*d+(-2*P2_2-2*P1_2)*b+(-P2_1-P1_1)*a+P2_3+P2_1+P1_3+P1_1)
                +P1_1*IntSharp.Math.Ln(P1_3*d+2*P1_2*b+P1_1*a)
                -P1_1*IntSharp.Math.Ln(-P1_3*d-2*P1_2*b-P1_1*a+P1_3+P1_1);
            
            var diffGradB = 
                (-2*P2_2-2*P1_2)*IntSharp.Math.Ln((P2_3+P1_3)*d+(2*P2_2+2*P1_2)*b+(P2_1+P1_1)*a)
                +2*P2_2*IntSharp.Math.Ln(P2_3*d+2*P2_2*b+P2_1*a)
                -2*P2_2*IntSharp.Math.Ln(-P2_3*d-2*P2_2*b-P2_1*a+P2_3+P2_1)
                +(2*P2_2+2*P1_2)*IntSharp.Math.Ln((-P2_3-P1_3)*d+(-2*P2_2-2*P1_2)*b+(-P2_1-P1_1)*a+P2_3+P2_1+P1_3+P1_1)
                +2*P1_2*IntSharp.Math.Ln(P1_3*d+2*P1_2*b+P1_1*a)
                -2*P1_2*IntSharp.Math.Ln(-P1_3*d-2*P1_2*b-P1_1*a+P1_3+P1_1);
            
            var diffGradD = 
                (-P2_3-P1_3)*IntSharp.Math.Ln((P2_3+P1_3)*d+(2*P2_2+2*P1_2)*b+(P2_1+P1_1)*a)
                +P2_3*IntSharp.Math.Ln(P2_3*d+2*P2_2*b+P2_1*a)
                -P2_3*IntSharp.Math.Ln(-P2_3*d-2*P2_2*b-P2_1*a+P2_3+P2_1)
                +(P2_3+P1_3)*IntSharp.Math.Ln((-P2_3-P1_3)*d+(-2*P2_2-2*P1_2)*b+(-P2_1-P1_1)*a+P2_3+P2_1+P1_3+P1_1)
                +P1_3*IntSharp.Math.Ln(P1_3*d+2*P1_2*b+P1_1*a)
                -P1_3*IntSharp.Math.Ln(-P1_3*d-2*P1_2*b-P1_1*a+P1_3+P1_1);
            
            return new List<Interval>{diffGradA, diffGradB, diffGradD};
        }

        public static double XLog(double a)
        {
            if (a <= 0.0) return 0.0;
            return a * System.Math.Log(a);
        }
        
        public static bool Contains(this Interval it, double value)
        {
            return it.Infimum <= value && it.Supremum >= value;
        }

        public static double MaxWidth(this IEnumerable<Interval> input)
        {
            var enumerable = input as List<Interval> ?? input.ToList();
            if (enumerable.Count == 0) return 0.0;
            var largestWidth = enumerable[0].Diam();
            foreach (var interval in enumerable.Where(interval => interval.Diam() >= largestWidth))
                largestWidth = interval.Diam();

            return largestWidth;
        }

        public static double MaxWidth(this IEnumerable<IEnumerable<Interval>> listOfVariables)
        {
            var enumerable = listOfVariables as List<IEnumerable<Interval>> ?? listOfVariables.ToList();
            if (enumerable.Count == 0) return 0.0;
            var largestWidth = enumerable[0].MaxWidth();
            foreach (var intervals in enumerable.Where(interval => MaxWidth(interval) >= largestWidth))
                largestWidth = intervals.MaxWidth();
            return largestWidth;
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