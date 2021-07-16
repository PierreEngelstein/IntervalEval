using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using IntervalEval.Core.Helpers;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace IntervalEval.Core.MutualInformationProblem
{
    public static class MutualInfoHelpers2Variables
    {
        // private static Mutex printMutex = new();
        private static readonly Interval It01 = Interval.FromInfSup(0, 1);
        public static Tuple<Interval, bool, int, IEnumerable<Interval>> MutualInformation(
            Interval a, Interval b, Interval d, 
            double y1A, double y1B, double y1D, double y2A, double y2B, double y2D, 
            double probabilityP1, double probabilityP2, double aPivot, double bPivot, double dPivot,
            bool isGradientToUse, int valSolGradient, IEnumerable<Interval> previousGradient)
        {
            // Note: removing useless code allows us to gain about 58% performance...
            
            // Compute interval using mean value form
            var grad = GradientMutualInfo(a, b, d, y1A, y1B, y1D, y2A, y2B, y2D).ToArray();
            var fX0 = MutualInformation(a.Infimum, b.Infimum, d.Infimum, y1A, y1B, y1D, y2A, y2B, y2D, probabilityP1, probabilityP2);
            var fMeanValueForm = fX0 + grad[0] * (a - a.Infimum) + grad[1] * (b - b.Infimum) + grad[2] * (d - d.Infimum);

            // Compute interval using natural inclusion function
            var fNaturalInclusion = MutualInformation(a, b, d,
                Interval.FromInfSup(y1A, y1A), Interval.FromInfSup(y1B, y1B), Interval.FromInfSup(y1D, y1D),
                Interval.FromInfSup(y2A, y2A), Interval.FromInfSup(y2B, y2B), Interval.FromInfSup(y2D, y2D), 
                probabilityP1, probabilityP2);

            // Compute interval using slope arithmetic
            var fSlopeInclusion = MutualInformationSlope(a, b, d,
                Interval.FromInfSup(y1A, y1A), Interval.FromInfSup(y1B, y1B), Interval.FromInfSup(y1D, y1D),
                Interval.FromInfSup(y2A, y2A), Interval.FromInfSup(y2B, y2B), Interval.FromInfSup(y2D, y2D),
                probabilityP1, probabilityP2, aPivot, bPivot, dPivot);
            
            // Compute the upper bound of mutual information using the polyhedron corners
            var maxCorner = MaxMutualInfoPolyhedronCorner(a, b, d,
                Interval.FromInfSup(y1A, y1A), Interval.FromInfSup(y1B, y1B), Interval.FromInfSup(y1D, y1D),
                Interval.FromInfSup(y2A, y2A), Interval.FromInfSup(y2B, y2B), Interval.FromInfSup(y2D, y2D),
                probabilityP1, probabilityP2);
            var fPolyhedronInclusion = Interval.FromInfSup(fNaturalInclusion.Infimum, maxCorner);
            

            // Intersect both evaluations to get smallest evaluation possible
            var fIntersected = fMeanValueForm.Intersection(fPolyhedronInclusion);
            if (fIntersected.IsEmpty()) fIntersected = fPolyhedronInclusion;

            // return new Tuple<Interval, bool, int, IEnumerable<Interval>>(fSlopeInclusion, false, -1, previousGradient);
            // return new Tuple<Interval, bool, int, IEnumerable<Interval>>(fPolyhedronInclusion, false, -1, previousGradient);
            // return new Tuple<Interval, bool, int, IEnumerable<Interval>>(fNaturalInclusion, false, -1, previousGradient);

            return new Tuple<Interval, bool, int, IEnumerable<Interval>>(fIntersected, false, -1, previousGradient);
        }

        /// <summary>
        /// Computes the mutual information of real numbers.
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
        /// Computes the mutual information of intervals using slope arithmetics.
        /// TODO: also check second-order interval slopes (Kolev, L.V. Use of Interval Slopes for the Irrational Part of Factorable Functions => cf section 2.2)
        /// </summary>
        private static Interval MutualInformationSlope(Interval a, Interval b, Interval d,
            Interval P1_1, Interval P1_2, Interval P1_3, Interval P2_1, Interval P2_2, Interval P2_3,
            double probabilityP1, double probabilityP2, double aPivot, double bPivot, double dPivot)
        {
            var p11 = a*P1_1+2*b*P1_2+d*P1_3;
            var p12 = (1-a)*P1_1-2*b*P1_2+(1-d)*P1_3;
            var p21 = a*P2_1+2*b*P2_2+d*P2_3;
            var p22 = (1-a)*P2_1-2*b*P2_2+(1-d)*P2_3;
            var p11p21 = (P2_3+P1_3)*d+(2*P2_2+2*P1_2)*b+(P2_1+P1_1)*a;
            var p12p22 = (-P2_3-P1_3)*d+(-2*P2_2-2*P1_2)*b+(-P2_1-P1_1)*a+P2_3+P2_1+P1_3+P1_1;

            var p11Pivot = aPivot*P1_1+2*bPivot*P1_2+dPivot*P1_3;
            var p12Pivot = (1-aPivot)*P1_1-2*bPivot*P1_2+(1-dPivot)*P1_3;
            var p21Pivot = aPivot*P2_1+2*bPivot*P2_2+dPivot*P2_3;
            var p22Pivot = (1-aPivot)*P2_1-2*bPivot*P2_2+(1-dPivot)*P2_3;
            var p11P21Pivot = (P2_3+P1_3)*dPivot+(2*P2_2+2*P1_2)*bPivot+(P2_1+P1_1)*aPivot;
            var p12P22Pivot = (-P2_3-P1_3)*dPivot+(-2*P2_2-2*P1_2)*bPivot+(-P2_1-P1_1)*aPivot+P2_3+P2_1+P1_3+P1_1;
            
            var slopeP11A = P1_1;
            var slopeP11B = 2*P1_2;
            var slopeP11D = P1_3;

            var slopeP12A = -P1_1;
            var slopeP12B = -2*P1_2;
            var slopeP12D = -P1_3;

            var slopeP21A = P2_1;
            var slopeP21B = 2*P2_2;
            var slopeP21D = P2_3;

            var slopeP22A = -P2_1;
            var slopeP22B = -2*P2_2;
            var slopeP22D = -P2_3;
            
            var slopeP11P21A = P2_1+P1_1;
            var slopeP11P21B = 2*P2_2+2*P1_2;
            var slopeP11P21D = P2_3+P1_3;
            
            var slopeP12P22A = -P2_1-P1_1;
            var slopeP12P22B = -2*P2_2-2*P1_2;
            var slopeP12P22D = -P2_3-P1_3;

            try
            {
                p11 = p11.Intersection(It01);
                p12 = p12.Intersection(It01);
                p21 = p21.Intersection(It01);
                p22 = p22.Intersection(It01);
                p11p21 = p11p21.Intersection(It01);
                p12p22 = p12p22.Intersection(It01);
            }
            catch (Exception)
            {
                return Interval.Empty;
            }

            var fy =
                - IntervalMath.XLog(p11p21.Infimum)
                - IntervalMath.XLog(p12p22.Infimum)
                + IntervalMath.XLog(p21.Infimum)
                + IntervalMath.XLog(p11.Infimum)
                + IntervalMath.XLog(p22.Infimum)
                + IntervalMath.XLog(p12.Infimum);
            
            // Compute the slope
            var slope = (a - aPivot) * (
                - IntervalMath.XLogSlope(p11P21Pivot.Mid(), p11p21) * slopeP11P21A
                - IntervalMath.XLogSlope(p12P22Pivot.Mid(), p12p22) * slopeP12P22A
                + IntervalMath.XLogSlope(p21Pivot.Mid(), p21) * slopeP21A
                + IntervalMath.XLogSlope(p11Pivot.Mid(), p11) * slopeP11A
                + IntervalMath.XLogSlope(p22Pivot.Mid(), p22) * slopeP22A
                + IntervalMath.XLogSlope(p12Pivot.Mid(), p12) * slopeP12A
                ) +
                 (b - bPivot) * (
                     - IntervalMath.XLogSlope(p11P21Pivot.Mid(), p11p21) * slopeP11P21B
                     - IntervalMath.XLogSlope(p12P22Pivot.Mid(), p12p22) * slopeP12P22B
                     + IntervalMath.XLogSlope(p21Pivot.Mid(), p21) * slopeP21B
                     + IntervalMath.XLogSlope(p11Pivot.Mid(), p11) * slopeP11B
                     + IntervalMath.XLogSlope(p22Pivot.Mid(), p22) * slopeP22B
                     + IntervalMath.XLogSlope(p12Pivot.Mid(), p12) * slopeP12B
                 ) +
                 (d - dPivot) * (
                     - IntervalMath.XLogSlope(p11P21Pivot.Mid(), p11p21) * slopeP11P21D
                     - IntervalMath.XLogSlope(p12P22Pivot.Mid(), p12p22) * slopeP12P22D
                     + IntervalMath.XLogSlope(p21Pivot.Mid(), p21) * slopeP21D
                     + IntervalMath.XLogSlope(p11Pivot.Mid(), p11) * slopeP11D
                     + IntervalMath.XLogSlope(p22Pivot.Mid(), p22) * slopeP22D
                     + IntervalMath.XLogSlope(p12Pivot.Mid(), p12) * slopeP12D
                 );
            
            /**
             *
             (u°v)' = (u'°v)*v'
             
             slope(u°v, pivot) = slope(u, v(pivot)) * slope(v, pivot)
             
             slope(cos(x²), pivot) => u: cos, v: x²
               => slope(cos, pivot²) * slope(x², pivot)
               => slope(cos, pivot²) * (x - pivot)
             
             u = InfoMutuelle
             v = Pij
             */

            var inclusionResult = fy + slope +(-IntervalMath.XLog(probabilityP1) - IntervalMath.XLog(probabilityP2));
            return inclusionResult;
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
            // Intersect each pij with [0, 1] to ensure the intervals be in correct bounds for the x*log(x) function
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
            
            // Natural inclusion form
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

        /// <summary>
        /// Computes the upper bound of the mutual information using polyhedron corners. The goal is to compute all the corners
        /// of the polyhedron composed of the equations { 0 \leq Pi_j \leq 1 }. We then evaluate the convex function on each corner
        /// and select the highest one as the upper bound.
        /// The corners selected have to be inside the ([a], [b], [d]) intervals to be valid, so we eliminate the one outside.
        ///
        /// To compute the corner, we create the 18x3 matrix corresponding to the polyhedron, and we get each 3x3 matrices. We then
        /// invert those squared matrices and multiply by the B vector. The result is the coordinates of the corner in the (a, b, d)
        /// space.
        /// </summary>
        private static double MaxMutualInfoPolyhedronCorner(Interval a, Interval b, Interval d,
            Interval P1_1, Interval P1_2, Interval P1_3, Interval P2_1, Interval P2_2, Interval P2_3,
            double probabilityP1, double probabilityP2)
        {
            // Polyhedron matrix
            var A = new double[18, 3]
            {
                {P1_1.Mid(),          (2*P1_2).Mid(),         P1_3.Mid()},
                {P1_1.Mid(),          (2*P1_2).Mid(),         P1_3.Mid()},
                {-P1_1.Mid(),        -(2*P1_2).Mid(),        -P1_3.Mid()},
                {-P1_1.Mid(),        -(2*P1_2).Mid(),        -P1_3.Mid()},
                {P2_1.Mid(),          (2*P2_2).Mid(),         P2_3.Mid()},
                {P2_1.Mid(),          (2*P2_2).Mid(),         P2_3.Mid()},
                {-P2_1.Mid(),        -(2*P2_2).Mid(),        -P2_3.Mid()},
                {-P2_1.Mid(),        -(2*P2_2).Mid(),        -P2_3.Mid()},
                {(P2_1+P1_1).Mid(),   (2*P2_2+2*P1_2).Mid(),  (P2_3+P1_3).Mid()},
                {(P2_1+P1_1).Mid(),   (2*P2_2+2*P1_2).Mid(),  (P2_3+P1_3).Mid()},
                {-(P2_1+P1_1).Mid(), -(2*P2_2+2*P1_2).Mid(), -(P2_3+P1_3).Mid()},
                {-(P2_1+P1_1).Mid(), -(2*P2_2+2*P1_2).Mid(), -(P2_3+P1_3).Mid()},
                {1,                   0,                       0},
                {1,                   0,                       0},
                {0,                   1,                       0},
                {0,                   1,                       0},
                {0,                   0,                       1},
                {0,                   0,                       1},
            };

            var vectB = new double[18]
            {
                0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, a.Infimum, a.Supremum, b.Infimum, b.Supremum, d.Infimum, d.Supremum
            };

            var largestMutualInfo = double.NegativeInfinity;

            // Get the 3x3 matrices inside the 18x3 matrix
            for (var i = 0; i < 18; i++)
            {
                var currentMatrix = Matrix<double>.Build.Dense(3, 3);
                currentMatrix[0, 0] = A[i, 0];
                currentMatrix[0, 1] = A[i, 1];
                currentMatrix[0, 2] = A[i, 2];
                for (var j = i+1; j < 18; j++)
                {
                    // if (i % 2 != 0 && j == i + 1) continue;
                    currentMatrix[1, 0] = A[j, 0];
                    currentMatrix[1, 1] = A[j, 1];
                    currentMatrix[1, 2] = A[j, 2];
                    for (var k = j+1; k < 18; k++)
                    {
                        // if (j % 2 != 0 && k == j + 1) continue;
                        currentMatrix[2, 0] = A[k, 0];
                        currentMatrix[2, 1] = A[k, 1];
                        currentMatrix[2, 2] = A[k, 2];
                        var det = currentMatrix.Determinant();
                        if (Math.Abs(det - 0.0) <= 1e-03) continue; // If determinant == 0 => matrix not invertible.
                        
                        // Compute b * inverse(currentMatrix)
                        var invert = currentMatrix.Inverse();
                        var currentVectorB = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.Dense(new[] {vectB[i], vectB[j], vectB[k]});
                        var result = invert.Multiply(currentVectorB);
                        // If the result is in bounds of the initial intervals, consider it correct and evaluate the mutual information on it.
                        if (a.ApproximatelyIn(result[0]) && b.ApproximatelyIn(result[1]) && d.ApproximatelyIn(result[2]))
                        {
                            // Evaluate mutual info on result[0], result[1], result[2] and keep the largest one
                            var curMutualInfo = MutualInformation(
                                Interval.FromInfSup(result[0], result[0]), Interval.FromInfSup(result[1], result[1]), Interval.FromInfSup(result[2], result[2]),
                                P1_1, P1_2, P1_3, P2_1, P2_2, P2_3,
                                probabilityP1, probabilityP2).Supremum;
                            if (curMutualInfo > largestMutualInfo) largestMutualInfo = curMutualInfo;
                        }
                    }
                }
            }

            if (double.IsNegativeInfinity(largestMutualInfo)) throw new Exception("I am -oo");

            return largestMutualInfo;
        }
    }
}