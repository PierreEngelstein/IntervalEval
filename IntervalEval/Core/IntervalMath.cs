using System;
using System.Collections.Generic;
using System.Linq;

namespace IntervalEval.Core
{
    /// <summary>
    /// Class that implements common mathematically functions
    /// </summary>
    public static class IntervalMath
    {
        // Todo: add argument reduction for sin/cos (binary calculations).
        // Todo: add more functions (tan, asin, ...)

        public static Interval Abs(Interval i)
        {
            var absInf = Math.Abs(i.Infimum);
            var absSup = Math.Abs(i.Supremum);

            return i.ContainsZero()
                ? Interval.FromInfSup(0, Math.Max(absInf, absSup))
                : Interval.FromInfSup(Math.Min(absInf, absSup), Math.Max(absInf, absSup));
        }

        /// <summary>
        /// Defined for -2Pi smaller/equal argument smaller/equal 2Pi.
        /// </summary>
        public static Interval Cos(Interval i)
        {
            if (i.Diam() > Interval.TwoPi) return Interval.FromInfSup(-1, 1);

            if (i > Interval.TwoPi || i.Infimum < -Interval.TwoPi)
                throw new Exception("Cosine only implemented for arguments between -2Pi and 2Pi.");

            var bounds = new List<double>
            {
                Math.Cos(i.Infimum).InflateDown(),
                Math.Cos(i.Supremum).InflateDown(),
                Math.Cos(i.Infimum).InflateUp(),
                Math.Cos(i.Supremum).InflateUp()
            };

            var infimum = bounds.Min();
            var supremum = bounds.Max();

            // Check critical points.
            if (i.ContainsZero() || Interval.TwoPi.Subset(i) || Interval.TwoPi.Subset(-i)) supremum = 1;
            if (Interval.Pi.Subset(i) || Interval.Pi.Subset(-i)) infimum = -1;

            return Interval.FromInfSup(infimum, supremum);
        }

        /// <summary>
        /// Defined for -2Pi smaller/equal argument smaller/equal 2Pi.
        /// </summary>
        public static IntervalVector Cos(IntervalVector i)
        {
            var results = new Interval[i.RowCount];
            for (var row = 0; row < i.RowCount; row++)
            {
                results[row] = Cos(i.Items[row]);
            }

            return new IntervalVector(results);
        }

        /// <summary>
        /// Defined for -2Pismaller/equal argument smaller/equal 2Pi.
        /// </summary>
        public static IntervalMatrix Cos(IntervalMatrix i)
        {
            var results = new Interval[i.RowCount,i.RowCount];
            for (var row = 0; row < i.RowCount; row++)
            {
                for (var col = 0; col < i.ColumnCount; col++)
                {
                    results[row,col] = Cos(i.Items[row,col]);
                }
                
            }

            return new IntervalMatrix(results);
        }
        
        public static Interval Exp(Interval i)
        {
            var inf = Math.Exp(i.Infimum).InflateDown();
            var sup = Math.Exp(i.Supremum).InflateUp();

            return Interval.FromInfSup(inf, sup);
        }

        /// <summary>
        /// Defined for positiv intervals and base >= 1.
        /// </summary>
        public static Interval Log(Interval i, double newBase)
        {
            // Filter negativ intervals.
            if (!i.IsPositive()) throw new Exception("Negativ arguments are invalid.");

            // Filter too small base.
            if (newBase < 1) throw new Exception("Bases smaller than 1 are invalid.");

            var inf = Math.Log(i.Infimum, newBase).InflateDown();
            var sup = Math.Log(i.Supremum, newBase).InflateUp();
            return Interval.FromInfSup(inf, sup);
        }

        /// <summary>
        /// Base e logarithm defined from -oo to +oo, returning -oo when x \leq 0 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static Interval Ln(Interval i)
        {
            var j = Interval.FromInfSup(Math.Max(0.0, i.Infimum), Math.Max(0.0, i.Supremum));
            if (j == Interval.Zero)
            {
                // Console.WriteLine("0, 0");
                return Interval.FromInfSup(Double.NegativeInfinity, Double.NegativeInfinity);
            }
            // Console.WriteLine($"{j.Infimum},  {j.Supremum}");
            // Console.WriteLine($"{System.Math.Log(j.Infimum)},  {System.Math.Log(j.Supremum)}");
            return Interval.FromInfSup(j.Infimum == 0.0 ? double.NegativeInfinity : Math.Log(j.Infimum), j.Supremum == 0.0 ? double.NegativeInfinity : Math.Log(j.Supremum));
        }

        // Todo: improve pown to pow -> support any exponent, not just integer.
        public static Interval Pown(Interval i, int exponent)
        {
            double inf;
            double sup;

            // Odd exponent.
            if (exponent % 2 != 0)
            {
                inf = Math.Pow(i.Infimum, exponent).InflateDown();
                sup = Math.Pow(i.Supremum, exponent).InflateUp();
                return Interval.FromInfSup(inf, sup);
            }

            // Even exponent.
            if (i.Infimum >= 0)
            {
                inf = Math.Pow(i.Infimum, exponent).InflateDown();
                sup = Math.Pow(i.Supremum, exponent).InflateUp();
                return Interval.FromInfSup(inf, sup);
            }
            if (i.Supremum < 0)
            {
                inf = Math.Pow(i.Supremum, exponent).InflateDown();
                sup = Math.Pow(i.Infimum, exponent).InflateUp();
                return Interval.FromInfSup(inf, sup);
            }

            var powerInf = Math.Pow(i.Infimum, exponent).InflateUp();
            var powerSup = Math.Pow(i.Supremum, exponent).InflateUp();
            inf = 0;
            sup = Math.Max(powerInf, powerSup);
            return Interval.FromInfSup(inf, sup);
        }

        /// <summary>
        /// Defined for -2Pi smaller/equal argument smaller/equal 2Pi.
        /// </summary>
        public static Interval Sin(Interval i)
        {
            if (i.Diam() > Interval.TwoPi) return Interval.FromInfSup(-1, 1);

            if (i > Interval.TwoPi || i.Infimum < -Interval.TwoPi)
                throw new Exception("Sine only implemented for arguments between -2Pi and 2Pi.");

            var bounds = new List<double>
            {
                Math.Sin(i.Infimum).InflateDown(),
                Math.Sin(i.Supremum).InflateDown(),
                Math.Sin(i.Infimum).InflateUp(),
                Math.Sin(i.Supremum).InflateUp()
            };

            var infimum = bounds.Min();
            var supremum = bounds.Max();

            // Check critical points.
            if (Interval.HalfPi.Subset(i) || Interval.ThreeHalfPi.Subset(-i)) supremum = 1;
            if (Interval.ThreeHalfPi.Subset(i) || Interval.HalfPi.Subset(-i)) infimum = -1;

            return Interval.FromInfSup(infimum, supremum);
        }

        /// <summary>
        /// Note that interval*interval != interval﷿.
        /// </summary>
        public static Interval Sqr(Interval i)
        {
            if (!i.ContainsZero()) return i * i;

            var absInf = Math.Abs(i.Infimum);
            var absSup = Math.Abs(i.Supremum);
            var absMax = Math.Max(absInf, absSup);

            var supremum = (absMax * absMax).InflateUp();
            return Interval.FromInfSup(0, supremum);
        }

        /// <summary>
        /// Defined for arguments >= 0.
        /// </summary>
        public static Interval Sqrt(Interval i)
        {
            if (i.Infimum < 0) throw new Exception("Negativ arguments are invalid.");

            var infimum = Math.Sqrt(i.Infimum).InflateDown();
            var supremum = Math.Sqrt(i.Supremum).InflateUp();

            return Interval.FromInfSup(infimum, supremum);
        }

        public static int AmountFailXLog;
        public const double ZeroPrecision = 1e-05;
        public static readonly double XLogMin = Math.Exp(-1);

        /** Computes the interval slope for the function x -> x*log(x) */
        public static Interval XLogSlope_Inf(Interval it)
        {
            return XLogSlope(it.Infimum, it);
        }        
        
        /** Computes the interval slope for the function x -> x*log(x) */
        public static Interval XLogSlope_Sup(Interval it)
        {
            return XLogSlope(it.Supremum, it);
        }
        
        //
        public static Interval XLogSlope(double pivot, Interval it)
        {
            if (pivot < 0) pivot = 0;
            // f'(a) = log(a) + 1
            var fzInf = XLog(pivot) + 1;
            //S(f, a , b)_sup = [H(b) - H(a)]/(b-a)
            var fzSup = (XLog(it.Supremum) - XLog(pivot)) / (it.Supremum - pivot);
            var fz = Interval.FromInfSup(fzInf, fzSup);

            return fz;
        }
 
        /** Custom method to evaluate f(x) = x*log(x) with f(0) = 0 instead of f(0) = -oo */
        public static Interval XLog(Interval it, string whereAmI = "")
        {
            // x \in [-oo, 0] => no result
            if (it.Supremum < 0)
            {
                // Console.WriteLine($"Encoutered undefined xlog at {whereAmI}");
                AmountFailXLog++;
                // return Interval.Zero;
                return Interval.Empty;
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
                return Interval.FromInfSup(it.Supremum * Math.Log(it.Supremum), it.Infimum * Math.Log(it.Infimum));
            }
            // x \in [0-, exp(-1)] => y strictly decreasing and max(y) = 0
            if (it.Infimum <= ZeroPrecision && it.Supremum <= XLogMin)
            {
                return Interval.FromInfSup(it.Supremum * Math.Log(it.Supremum), 0);
            }
            // x \in [0+, +oo], x <= exp(-1)  => y increases then decreases
            if (it.Infimum <= XLogMin && it.Supremum >= XLogMin && it.Infimum >= ZeroPrecision)
            {
                return Interval.FromInfSup(XLogMin * Math.Log(XLogMin),
                    Math.Max(it.Infimum * Math.Log(it.Infimum), it.Supremum * Math.Log(it.Supremum)));
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
                return Interval.FromInfSup(XLogMin * Math.Log(XLogMin),
                    Math.Max(0.0, it.Supremum * Math.Log(it.Supremum)));
            }
            // x \in [exp(-1), +oo] => y strictly increasing
            if (it.Infimum >= XLogMin)
            {
                return Interval.FromInfSup(it.Infimum * Math.Log(it.Infimum), it.Supremum * Math.Log(it.Supremum)); 
            }
            // No solution otherwise
            // Console.WriteLine($"Encoutered undefined xlog at {whereAmI}");
            AmountFailXLog++;
            return Interval.Zero;
        }

        public static double XLog(double a)
        {
            return a switch
            {
                0.0 => 0.0,
                < 0.0 => double.NaN,
                _ => a * Math.Log(a)
            };
        }
    }
}
