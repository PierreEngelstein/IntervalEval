using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IntervalEval.Core.MutualInformationProblem
{
    public static class MutualInfoHelpers3Variables
    {
        public static Tuple<Interval, bool, int, IEnumerable<Interval>> MutualInformation3Vars(Interval a1, Interval b1, Interval d1, Interval a2, Interval b2, Interval d2, double y1A,
            double y1B, double y1D, double y2A, double y2B, double y2D, double y3A, double y3B, double y3D, bool isGradientToUse, int valSolGradient, IEnumerable<Interval> previousGradient)
        {
            
            var valueSolutionGradient = -1;
            if (!isGradientToUse)
            {
                var gradient = GradientMutualInfo3Vars(a1, b1, d1, a2, b2, d2, y1A, y1B, y1D, y2A, y2B, y2D, y3A, y3B, y3D);
                var inputValues = new List<Interval> {a1, b1, d1, a2, b2, d2};
                var enumerable = gradient as Interval[] ?? Enumerable.ToArray<Interval>(gradient);
                var ok = false;
                for (var i = 0; i < Math.Pow(2, enumerable.Length); i++)
                {
                    var result = true;
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

        private static Interval MutualInformation3Vars(Interval a1, Interval b1, Interval d1, Interval a2,
            Interval b2, Interval d2, double y1A,
            double y1B, double y1D, double y2A, double y2B, double y2D, double y3A, double y3B, double y3D)
        {
            var result = 
                -IntervalMath.XLog(d2*y3D+2*b2*y3B+a2*y3A+d2*y2D+2*b2*y2B+a2*y2A+d2*y1D+2*b2*y1B+a2*y1A, "EntropyM")
                - IntervalMath.XLog(d1*y3D+2*b1*y3B+a1*y3A+d1*y2D+2*b1*y2B+a1*y2A+d1*y1D+2*b1*y1B+a1*y1A, "EntropyM")
                - IntervalMath.XLog((-d2-d1+1)*y3D+2*(-b2-b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+2*(-b2-b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+2*(-b2-b1)*y1B+(-a2-a1+1)*y1A, "EntropyM")
                + IntervalMath.XLog(d2*y3D+2*b2*y3B+a2*y3A, "Joint Entropy (32)")
                + IntervalMath.XLog((-d2-d1+1)*y3D+2*(-b2-b1)*y3B+(-a2-a1+1)*y3A, "Joint Entropy (33)")
                + IntervalMath.XLog(d1*y3D+2*b1*y3B+a1*y3A, "Joint Entropy (31)")
                + IntervalMath.XLog(d2*y2D+2*b2*y2B+a2*y2A, "Joint Entropy (22)")
                + IntervalMath.XLog((-d2-d1+1)*y2D+2*(-b2-b1)*y2B+(-a2-a1+1)*y2A, "Joint Entropy (23)")
                + IntervalMath.XLog(d1*y2D+2*b1*y2B+a1*y2A, "Joint Entropy (21)")
                + IntervalMath.XLog(d2*y1D+2*b2*y1B+a2*y1A, "Joint Entropy (12)")
                + IntervalMath.XLog((-d2-d1+1)*y1D+2*(-b2-b1)*y1B+(-a2-a1+1)*y1A, "Joint Entropy (13)")
                + IntervalMath.XLog(d1*y1D+2*b1*y1B+a1*y1A, "Joint Entropy (11)") + (-IntervalMath.XLog(0.1) - IntervalMath.XLog(0.6) - IntervalMath.XLog(0.3));
            return result;
        }

        private static double MutualInformation3Vars(double a1, double b1, double d1, double a2, double b2, double d2, double y1A,
            double y1B, double y1D, double y2A, double y2B, double y2D, double y3A, double y3B, double y3D)
        {
            var result = 
                -IntervalMath.XLog(d2*y3D+2*b2*y3B+a2*y3A+d2*y2D+2*b2*y2B+a2*y2A+d2*y1D+2*b2*y1B+a2*y1A)
                - IntervalMath.XLog(d1*y3D+2*b1*y3B+a1*y3A+d1*y2D+2*b1*y2B+a1*y2A+d1*y1D+2*b1*y1B+a1*y1A)
                - IntervalMath.XLog((-d2-d1+1)*y3D+2*(-b2-b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+2*(-b2-b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+2*(-b2-b1)*y1B+(-a2-a1+1)*y1A)
                + IntervalMath.XLog(d2*y3D+2*b2*y3B+a2*y3A)
                + IntervalMath.XLog((-d2-d1+1)*y3D+2*(-b2-b1)*y3B+(-a2-a1+1)*y3A)
                + IntervalMath.XLog(d1*y3D+2*b1*y3B+a1*y3A)
                + IntervalMath.XLog(d2*y2D+2*b2*y2B+a2*y2A)
                + IntervalMath.XLog((-d2-d1+1)*y2D+2*(-b2-b1)*y2B+(-a2-a1+1)*y2A)
                + IntervalMath.XLog(d1*y2D+2*b1*y2B+a1*y2A)
                + IntervalMath.XLog(d2*y1D+2*b2*y1B+a2*y1A)
                + IntervalMath.XLog((-d2-d1+1)*y1D+2*(-b2-b1)*y1B+(-a2-a1+1)*y1A)
                + IntervalMath.XLog(d1*y1D+2*b1*y1B+a1*y1A) + (-IntervalMath.XLog(0.1) - IntervalMath.XLog(0.6) - IntervalMath.XLog(0.3));;
            return result;
        }

        private static IEnumerable<Interval> GradientMutualInfo3Vars(Interval a1, Interval b1, Interval d1, Interval a2,
            Interval b2, Interval d2, double y1A,
            double y1B, double y1D, double y2A, double y2B, double y2D, double y3A, double y3B, double y3D)
        {
            var diffA1 = 
                (y3A+y2A+y1A)*IntervalMath.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                +(-y3A-y2A-y1A)*IntervalMath.Ln(d1*y3D+2*b1*y3B+a1*y3A+d1*y2D+2*b1*y2B+a1*y2A+d1*y1D+2*b1*y1B+a1*y1A)
                -y3A*IntervalMath.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A)
                +y3A*IntervalMath.Ln(d1*y3D+2*b1*y3B+a1*y3A)
                -y2A*IntervalMath.Ln((-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A)
                +y2A*IntervalMath.Ln(d1*y2D+2*b1*y2B+a1*y2A)
                -y1A*IntervalMath.Ln((-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                +y1A*IntervalMath.Ln(d1*y1D+2*b1*y1B+a1*y1A);
            var diffB1 =
                (2*y3B+2*y2B+2*y1B)*IntervalMath.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                -2*y3B*IntervalMath.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A)
                +(-2*y3B-2*y2B-2*y1B)*IntervalMath.Ln(d1*y3D+2*b1*y3B+a1*y3A+d1*y2D+2*b1*y2B+a1*y2A+d1*y1D+2*b1*y1B+a1*y1A)
                +2*y3B*IntervalMath.Ln(d1*y3D+2*b1*y3B+a1*y3A)
                -2*y2B*IntervalMath.Ln((-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A)
                +2*y2B*IntervalMath.Ln(d1*y2D+2*b1*y2B+a1*y2A)
                -2*y1B*IntervalMath.Ln((-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                +2*y1B*IntervalMath.Ln(d1*y1D+2*b1*y1B+a1*y1A);
            var diffD1 =
                (y3D+y2D+y1D)*IntervalMath.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                -y3D*IntervalMath.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A)
                +(-y3D-y2D-y1D)*IntervalMath.Ln(d1*y3D+2*b1*y3B+a1*y3A+d1*y2D+2*b1*y2B+a1*y2A+d1*y1D+2*b1*y1B+a1*y1A)
                +y3D*IntervalMath.Ln(d1*y3D+2*b1*y3B+a1*y3A)
                -y2D*IntervalMath.Ln((-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A)
                +y2D*IntervalMath.Ln(d1*y2D+2*b1*y2B+a1*y2A)
                -y1D*IntervalMath.Ln((-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                +y1D*IntervalMath.Ln(d1*y1D+2*b1*y1B+a1*y1A);
            var diffA2 = 
                (-y3A-y2A-y1A)*IntervalMath.Ln(d2*y3D+2*b2*y3B+a2*y3A+d2*y2D+2*b2*y2B+a2*y2A+d2*y1D+2*b2*y1B+a2*y1A)
                +y3A*IntervalMath.Ln(d2*y3D+2*b2*y3B+a2*y3A)
                +(y3A+y2A+y1A)*IntervalMath.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                -y3A*IntervalMath.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A)
                +y2A*IntervalMath.Ln(d2*y2D+2*b2*y2B+a2*y2A)
                -y2A*IntervalMath.Ln((-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A)
                +y1A*IntervalMath.Ln(d2*y1D+2*b2*y1B+a2*y1A)
                -y1A*IntervalMath.Ln((-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A);
            var diffB2 =
                (-2*y3B-2*y2B-2*y1B)*IntervalMath.Ln(d2*y3D+2*b2*y3B+a2*y3A+d2*y2D+2*b2*y2B+a2*y2A+d2*y1D+2*b2*y1B+a2*y1A)
                +2*y3B*IntervalMath.Ln(d2*y3D+2*b2*y3B+a2*y3A)
                +(2*y3B+2*y2B+2*y1B)*IntervalMath.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                -2*y3B*IntervalMath.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A)
                +2*y2B*IntervalMath.Ln(d2*y2D+2*b2*y2B+a2*y2A)
                -2*y2B*IntervalMath.Ln((-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A)
                +2*y1B*IntervalMath.Ln(d2*y1D+2*b2*y1B+a2*y1A)
                -2*y1B*IntervalMath.Ln((-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A);
            var diffD2 = 
                (-y3D-y2D-y1D)*IntervalMath.Ln(d2*y3D+2*b2*y3B+a2*y3A+d2*y2D+2*b2*y2B+a2*y2A+d2*y1D+2*b2*y1B+a2*y1A)
                +y3D*IntervalMath.Ln(d2*y3D+2*b2*y3B+a2*y3A)
                +(y3D+y2D+y1D)*IntervalMath.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A+(-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A+(-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A)
                -y3D*IntervalMath.Ln((-d2-d1+1)*y3D+(-2*b2-2*b1)*y3B+(-a2-a1+1)*y3A)
                +y2D*IntervalMath.Ln(d2*y2D+2*b2*y2B+a2*y2A)
                -y2D*IntervalMath.Ln((-d2-d1+1)*y2D+(-2*b2-2*b1)*y2B+(-a2-a1+1)*y2A)
                +y1D*IntervalMath.Ln(d2*y1D+2*b2*y1B+a2*y1A)
                -y1D*IntervalMath.Ln((-d2-d1+1)*y1D+(-2*b2-2*b1)*y1B+(-a2-a1+1)*y1A);
            return new List<Interval>{diffA1, diffB1, diffD1, diffA2, diffB2, diffD2};
        }
    }
}