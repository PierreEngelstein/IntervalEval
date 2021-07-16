using System;
using System.Collections.Generic;
using System.Linq;
using IntervalEval.Core.Helpers;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Utility = IntervalEval.Core.Utility;

namespace IntervalEval.Core
{
    /// <summary>
    /// Contains multiple convinience methods for using the interval type
    /// </summary>
    public static class Utility
    {
        // Todo: add more support for matrix/vector

        /// <summary>
        /// Checks wether the interval contains 0.
        /// </summary>
        public static bool ContainsZero(this Interval i)
        {
            return i.Infimum <= 0 && i.Supremum >= 0;
        }

        /// <summary>
        /// Checks wether the intervals have not a single point in common.
        /// </summary>
        public static bool Disjoint(this Interval lhs, Interval rhs)
        {
            return lhs.Supremum < rhs.Infimum || lhs.Infimum > rhs.Supremum;
        }

        /// <summary>
        /// Checks wether the interval contains a specific point (borders allowed).
        /// </summary>
        public static bool In(this Interval i, double a)
        {
            return i.Infimum <= a && i.Supremum >= a;
        }

        public static bool ApproximatelyIn(this Interval i, double a, double precision = 1e-05)
        {
            return i.Infimum <= a+precision  && i.Supremum >= a-precision;
        }
        
        public static IntervalBoolean In(this Interval lhs, Interval rhs)
        {
            if (lhs.Disjoint(rhs)) return IntervalBoolean.False;
            return lhs.Subset(rhs) ? IntervalBoolean.True : IntervalBoolean.Indeterminate;
        }

        /// <summary>
        /// Checks wether the first interval lies in the second (borders not allowed).
        /// </summary>
        public static bool Interior(this Interval lhs, Interval rhs)
        {
            return lhs.Infimum > rhs.Infimum && lhs.Supremum < rhs.Supremum;
        }

        /// <summary>
        /// Checks wether the matrix is interior of the other matrix (bounds not allowed).
        /// </summary>
        public static bool Interior(this IntervalMatrix lhs, IntervalMatrix rhs)
        {
            for (var row = 0; row < lhs.RowCount; row++)
            {
                for (var col = 0; col < lhs.ColumnCount; col++)
                {
                    if (!lhs.Items[row, col].Interior(rhs.Items[row, col]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks wether the vector is interior of the other vector (bounds not allowed).
        /// </summary>
        public static bool Interior(this IntervalVector lhs, IntervalVector rhs)
        {
            for (var row = 0; row < lhs.RowCount; row++)
            {
                if (!lhs.Items[row].Interior(rhs.Items[row]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks wether the interval is [-inf , +inf].
        /// </summary>
        public static bool IsEntire(this Interval i)
        {
            return double.IsNegativeInfinity(i.Infimum) && double.IsPositiveInfinity(i.Supremum);
        }

        /// <summary>
        /// Checks whether the interval is empty ([NaN, NaN])
        /// </summary>
        public static bool IsEmpty(this Interval i)
        {
            return double.IsNaN(i.Infimum) && double.IsNaN(i.Supremum);
        }

        /// <summary>
        /// Checks wether the whole interval is smaller than zero.
        /// </summary>
        public static bool IsNegative(this Interval i)
        {
            return i.Supremum < 0;
        }

        /// <summary>
        /// Checks wether the whole interval is greater than zero.
        /// </summary>
        public static bool IsPositive(this Interval i)
        {
            return i.Infimum > 0;
        }

        /// <summary>
        /// Checks wether the interval is [0 , 0].
        /// </summary>
        public static bool IsZero(this Interval i)
        {
            return i.Infimum == 0 && i.Supremum == 0;
        }

        /// <summary>
        /// Returns the supremum of the interval's absolute value.
        /// </summary>
        public static double Mag(this Interval i)
        {
            return IntervalMath.Abs(i).Supremum;
        }

        /// <summary>
        /// Returns the point in the middle of the interval.
        /// </summary>
        public static double Mid(this Interval i)
        {
            if (IsEntire(i)) return 0;
            return (i.Infimum + i.Supremum) / 2;
        }

        /// <summary>
        /// Converts an IntervalMatrix to a Math.NET Matrix(double) containing 
        /// the intervals' mid points.
        /// </summary>
        public static Matrix<double> Mid(this IntervalMatrix i)
        {
            var items = new double[i.RowCount, i.ColumnCount];
            for (var row = 0; row < i.RowCount; row++)
            {
                for (var col = 0; col < i.ColumnCount; col++)
                {
                    items[row, col] = i.Items[row, col].Mid();
                }
            }
            return Matrix.Build.DenseOfArray(items);
        }

        /// <summary>
        /// Converts an IntervalVector to a Math.NET Vecto(double) containing 
        /// the intervals' mid points.
        /// </summary>
        public static Vector<double> Mid(this IntervalVector i)
        {
            var items = new double[i.RowCount];
            for (var row = 0; row < i.RowCount; row++)
            {
                    items[row] = i.Items[row].Mid();
            }
            return Vector.Build.DenseOfArray(items);
        }

        /// <summary>
        /// Returns the infimum of the interval's absolute value.
        /// </summary>
        public static double Mig(this Interval i)
        {
            return IntervalMath.Abs(i).Infimum;
        }

        /// <summary>
        /// Returns the distance between the interval's mid and one of it's bounds.
        /// </summary>
        public static double Rad(this Interval i)
        {
            return (i.Supremum - i.Infimum) / 2;
        }

        /// <summary>
        /// Checks wether the first interval lies in the second (borders allowed).
        /// </summary>
        public static bool Subset(this Interval lhs, Interval rhs)
        {
            return lhs.Infimum >= rhs.Infimum && lhs.Supremum <= rhs.Supremum;
        }

        /// <summary>
        /// Returns the distance between the interval's lower and upper bound.
        /// </summary>
        public static double Diam(this Interval i)
        {
            return i.Supremum - i.Infimum;
        }

        /// <summary>
        /// Cuts the interval's supremum at the given value and returns a new interval that so contains
        /// no values greater than the given value. Can be used to truncate values that are logically not
        /// possible, for example a coefficient of determination greater than 1.
        /// </summary>
        public static Interval TruncateSup(this Interval i, double sup)
        {
            return i.Supremum <= sup ? i : Interval.FromInfSup(i.Infimum, sup);
        }

        /// <summary>
        /// Cuts the interval's infimum at the given value and returns a new interval that so contains
        /// no values smaller than the given value. Can be used to truncate values that are logically not
        /// possible, for example a coefficient of determination smaller than 0.
        /// </summary>
        public static Interval TruncateInf(this Interval i, double inf)
        {
            return i.Infimum >= inf ? i : Interval.FromInfSup(inf, i.Supremum);
        }

        /// <summary>
        /// Splits an interval at its mid point in two intervals.
        /// [ inf sup ] --> [ inf , mid ] and [ mid , sup ]
        /// </summary>
        public static Tuple<Interval, Interval> Bisect(this Interval i)
        {
            var l = Interval.FromInfSup(i.Infimum, i.Mid());
            var r = Interval.FromInfSup(i.Mid(), i.Supremum);

            return new Tuple<Interval, Interval>(l,r);
        }
        
        public static Tuple<Interval, Interval> Bisect(this Interval i, double percent)
        {
            var middle = i.Infimum + percent * (i.Supremum - i.Infimum);
            var l = Interval.FromInfSup(i.Infimum, middle);
            var r = Interval.FromInfSup(middle, i.Supremum);

            return new Tuple<Interval, Interval>(l,r);
        }
        
        /// <summary>
        /// Bisects a box in the middle by its largest width
        /// </summary>
        /// <param name="input">Box input</param>
        /// <returns>Left and right boxes</returns>
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
        
        /// <summary>
        /// Bisects a box by a certain percentage, by its largest width
        /// </summary>
        /// <param name="input">Box input</param>
        /// <param name="percent">Where to cut ?</param>
        /// <returns>Left and right boxes</returns>
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
        
        /// <summary>
        /// Gets the middle point of a box.
        /// </summary>
        /// <param name="input">Box input</param>
        /// <returns>Box containing the middle point (each interval has low_bound=maximum=middle)</returns>
        public static IEnumerable<Interval> Middle(this IEnumerable<Interval> input)
        {
            return input.Select(interval => Interval.FromInfSup(interval.Mid(), interval.Mid())).ToList();
        }

        /// <summary>
        /// Returns the intersection of the two intervals: The interval that both intervals have in common.
        /// </summary>
        public static Interval Intersection(this Interval i, Interval otherInterval)
        {
            if(i.Disjoint(otherInterval)) 
                return Interval.Empty;
                // throw new Exception("Intersection is empty because the intervals are disjoint.");

            var infimum = Math.Max(i.Infimum, otherInterval.Infimum);
            var supremum = Math.Min(i.Supremum, otherInterval.Supremum);

            return Interval.FromInfSup(infimum, supremum);
        }
        
        /// <summary>
        /// Cuts the input interval into n intervals of same width.
        /// </summary>
        /// <param name="input">Interval to be cut</param>
        /// <param name="n">Number of resulting intervals</param>
        public static List<Interval> Cut(this Interval input, int n)
        {
            var delta = input.Diam() / n;
            var current = input.Infimum;
            var intervals = new List<Interval>();
            while (current + delta <= input.Supremum)
            {
                intervals.Add(Interval.FromInfSup(current, current + delta));
                current += delta;
            }

            return intervals;
        }

        public static double MaxWidth(this IEnumerable<IEnumerable<Interval>> listOfVariables)
        {
            var enumerable = listOfVariables as List<IEnumerable<Interval>> ?? listOfVariables.ToList();
            if (enumerable.Count == 0) return 0.0;
            var largestWidth = enumerable[0].MaxWidth();
            foreach (var intervals in enumerable.Where(interval => Utility.MaxWidth(interval) >= largestWidth))
                largestWidth = intervals.MaxWidth();
            return largestWidth;
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
    }
}
