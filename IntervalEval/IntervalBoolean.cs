using System;
using IntSharp.Types;

namespace IntervalEval
{
    /// <summary>
    /// Implements Interval Booleans logic. Based on [Luc Jaulin, Michel Kieffer, Olivier Didrit, and Eric Walter.Applied IntervalAnalysis. Springer, 2001]
    /// </summary>
    public class IntervalBoolean
    {
        private int Infimum { get; }
        private int Supremum { get; }
        
        public static readonly IntervalBoolean False         = new(0, 0);
        public static readonly IntervalBoolean True          = new(1, 1);
        public static readonly IntervalBoolean Indeterminate = new(0, 1);
        
        private IntervalBoolean(int inf, int sup)
        {
            Infimum = inf;
            Supremum = sup;
        }

        public static bool operator==(IntervalBoolean left, IntervalBoolean right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            if (ReferenceEquals(right, null)) return false;
            return left.Infimum == right.Infimum && left.Supremum == right.Supremum;
        }

        public static bool operator !=(IntervalBoolean left, IntervalBoolean right)
        {
            return !(left == right);
        }

        public static IntervalBoolean operator &(IntervalBoolean left, IntervalBoolean right)
        {
            return left.And(right);
        }

        public static bool operator false(IntervalBoolean left)
        {
            return left == False;
        }

        public static bool operator true(IntervalBoolean left)
        {
            return left == True;
        }

        public static IntervalBoolean operator |(IntervalBoolean left, IntervalBoolean right)
        {
            return left.Or(right);
        }

        public static IntervalBoolean operator !(IntervalBoolean a)
        {
            return a.Not();
        }

        public static IntervalBoolean operator ^(IntervalBoolean right, IntervalBoolean left)
        {
            return right && !left || !right && left;
        }

        // [this] AND [left]
        private IntervalBoolean And(IntervalBoolean left)
        {
            if (this == False) return False;
            if (this == True) return left;
            if (this == Indeterminate)
            {
                return left == False ? False : Indeterminate;
            }

            return Indeterminate;
        }

        // [this] OR [left]
        private IntervalBoolean Or(IntervalBoolean left)
        {
            if (this == False) return left;
            if (this == True) return True;
            if (this == Indeterminate)
            {
                if (left == True) return True;
                return Indeterminate;
            }
            return Indeterminate;
        }

        public IntervalBoolean Not()
        {
            if (this == True) return False;
            if (this == False) return True;
            return Indeterminate;
        }
        
        private bool Equals(IntervalBoolean other)
        {
            return Infimum == other.Infimum && Supremum == other.Supremum;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((IntervalBoolean) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Infimum, Supremum);
        }

        public override string ToString()
        {
            if (this == True) return "true";
            if (this == False) return "false";
            if (this == Indeterminate) return "indeterminate";
            return "impossible";
        }
    }
}