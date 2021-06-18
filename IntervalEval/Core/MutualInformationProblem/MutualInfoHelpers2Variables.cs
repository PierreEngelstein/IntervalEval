using System;
using System.Collections.Generic;
using System.Linq;

namespace IntervalEval.Core.MutualInformationProblem
{
    public static class MutualInfoHelpers2Variables
    {
        private static readonly Interval It01 = Interval.FromInfSup(0, 1);
        public static Tuple<Interval, bool, int, IEnumerable<Interval>> MutualInformation(Interval a, Interval b, Interval d, double y1A, double y1B, double y1D, double y2A, double y2B, double y2D, double probabilityP1, double probabilityP2, bool isGradientToUse, int valSolGradient, IEnumerable<Interval> previousGradient)
        {
            // Compute interval using mean value form
            var grad = GradientMutualInfo(a, b, d, y1A, y1B, y1D, y2A, y2B, y2D).ToArray();
            var fX0 = MutualInformation(a.Infimum, b.Infimum, d.Infimum, y1A, y1B, y1D, y2A, y2B, y2D, probabilityP1, probabilityP2);
            var fMeanValueForm = fX0 + grad[0] * (a - a.Infimum) + grad[1] * (b - b.Infimum) + grad[2] * (d - d.Infimum);

            // Compute interval using natural inclusion function
            var fNaturalInclusion = MutualInformation(a, b, d,
                Interval.FromInfSup(y1A, y1A), Interval.FromInfSup(y1B, y1B), Interval.FromInfSup(y1D, y1D),
                Interval.FromInfSup(y2A, y2A), Interval.FromInfSup(y2B, y2B), Interval.FromInfSup(y2D, y2D), 
                probabilityP1, probabilityP2);

            // Intersect both evaluations to get smallest evaluation possible
            var fIntersected = fMeanValueForm.Intersection(fNaturalInclusion);
            if (fIntersected.IsEmpty()) fIntersected = fNaturalInclusion;

            return new Tuple<Interval, bool, int, IEnumerable<Interval>>(fIntersected, false, -1, previousGradient);
        }
        
        /// <summary>
        /// Computes the mutual information of numbers.
        /// </summary>
        private static double MutualInformation(double a, double b, double d,
            double p1A, double p1B, double p1D, double p2A, double p2B, double p2D, double probabilityP1, double probabilityP2)
        {
            return 
                -IntervalMath.XLog(p2D*d+p1D*d+2*p2B*b+2*p1B*b+p2A*a+p1A*a)
                -IntervalMath.XLog(p2D*(1-d)+p1D*(1-d)-2*p2B*b-2*p1B*b+p2A*(1-a)+p1A*(1-a))
                +IntervalMath.XLog(p2D*d+2*p2B*b+p2A*a)
                +IntervalMath.XLog(p1D*d+2*p1B*b+p1A*a)
                +IntervalMath.XLog(p2D*(1-d)-2*p2B*b+p2A*(1-a))
                +IntervalMath.XLog(p1D*(1-d)-2*p1B*b+p1A*(1-a))
                +(-IntervalMath.XLog(probabilityP1) - IntervalMath.XLog(probabilityP2));
        }

        /// <summary>
        /// Computes the mutual information of intervals.
        /// </summary>
        private static Interval MutualInformation(Interval a, Interval b, Interval d,
            Interval P1_1, Interval P1_2, Interval P1_3, Interval P2_1, Interval P2_2, Interval P2_3, double probabilityP1, double probabilityP2)
        {
            var p11 = a*P1_1+2*b*P1_2+d*P1_3;
            var p12 = (1-a)*P1_1-2*b*P1_2+(1-d)*P1_3;
            var p21 = a*P2_1+2*b*P2_2+d*P2_3;
            var p22 = (1-a)*P2_1-2*b*P2_2+(1-d)*P2_3;
            var p11p21 = (P2_3+P1_3)*d+(2*P2_2+2*P1_2)*b+(P2_1+P1_1)*a;
            var p12p22 = (-P2_3-P1_3)*d+(-2*P2_2-2*P1_2)*b+(-P2_1-P1_1)*a+P2_3+P2_1+P1_3+P1_1;

            try
            {
                p11 = p11.Intersection(It01);
                p12 = p12.Intersection(It01);
                p21 = p21.Intersection(It01);
                p22 = p22.Intersection(It01);
                p11p21 = p11p21.Intersection(It01);
                p12p22 = p12p22.Intersection(It01);
            }
            catch (Exception e)
            {
                return Interval.Empty;
            }

            return 
                -IntervalMath.XLog(p11p21, "Entropy p11+p12")
                -IntervalMath.XLog(p12p22, "Entropy p12+p22")
                +IntervalMath.XLog(p21,    "Joint Entropy (21)")
                +IntervalMath.XLog(p11,    "Joint Entropy (11)")
                +IntervalMath.XLog(p22,    "Joint Entropy (22)")
                +IntervalMath.XLog(p12,    "Joint Entropy (12)")
                +(-IntervalMath.XLog(probabilityP1) - IntervalMath.XLog(probabilityP2));
        }

        /// <summary>
        /// Computes the gradient of the mutual information, obtained with wxMaxima.
        /// grad(MI) = (d(MI)/da , d(MI)/db , d(MI)/dd).
        /// </summary>
        private static IEnumerable<Interval> GradientMutualInfo(Interval a, Interval b, Interval d,
            double P1_1, double P1_2, double P1_3, double P2_1, double P2_2, double P2_3)
        {
            var diffGradA = 
                (-P2_1-P1_1)*IntervalMath.Ln((P2_3+P1_3)*d+(2*P2_2+2*P1_2)*b+(P2_1+P1_1)*a)
                +P2_1*IntervalMath.Ln(P2_3*d+2*P2_2*b+P2_1*a)
                -P2_1*IntervalMath.Ln(-P2_3*d-2*P2_2*b-P2_1*a+P2_3+P2_1)
                +(P2_1+P1_1)*IntervalMath.Ln((-P2_3-P1_3)*d+(-2*P2_2-2*P1_2)*b+(-P2_1-P1_1)*a+P2_3+P2_1+P1_3+P1_1)
                +P1_1*IntervalMath.Ln(P1_3*d+2*P1_2*b+P1_1*a)
                -P1_1*IntervalMath.Ln(-P1_3*d-2*P1_2*b-P1_1*a+P1_3+P1_1);

            var diffGradB = 
                (-2*P2_2-2*P1_2)*IntervalMath.Ln((P2_3+P1_3)*d+(2*P2_2+2*P1_2)*b+(P2_1+P1_1)*a)
                +2*P2_2*IntervalMath.Ln(P2_3*d+2*P2_2*b+P2_1*a)
                -2*P2_2*IntervalMath.Ln(-P2_3*d-2*P2_2*b-P2_1*a+P2_3+P2_1)
                +(2*P2_2+2*P1_2)*IntervalMath.Ln((-P2_3-P1_3)*d+(-2*P2_2-2*P1_2)*b+(-P2_1-P1_1)*a+P2_3+P2_1+P1_3+P1_1)
                +2*P1_2*IntervalMath.Ln(P1_3*d+2*P1_2*b+P1_1*a)
                -2*P1_2*IntervalMath.Ln(-P1_3*d-2*P1_2*b-P1_1*a+P1_3+P1_1);

            var diffGradD = 
                (-P2_3-P1_3)*IntervalMath.Ln((P2_3+P1_3)*d+(2*P2_2+2*P1_2)*b+(P2_1+P1_1)*a)
                +P2_3*IntervalMath.Ln(P2_3*d+2*P2_2*b+P2_1*a)
                -P2_3*IntervalMath.Ln(-P2_3*d-2*P2_2*b-P2_1*a+P2_3+P2_1)
                +(P2_3+P1_3)*IntervalMath.Ln((-P2_3-P1_3)*d+(-2*P2_2-2*P1_2)*b+(-P2_1-P1_1)*a+P2_3+P2_1+P1_3+P1_1)
                +P1_3*IntervalMath.Ln(P1_3*d+2*P1_2*b+P1_1*a)
                -P1_3*IntervalMath.Ln(-P1_3*d-2*P1_2*b-P1_1*a+P1_3+P1_1);

            return new List<Interval>{diffGradA, diffGradB, diffGradD};
        }
    }
}