using System;
using NUnit.Framework;

namespace IntervalEval.Tests
{
    public class Tests
    {
        [Test]
        public void TestAnd()
        {
            var bool1 = IntervalBoolean.True;
            var bool2 = IntervalBoolean.True;
            var bool3 = bool1 && bool2;
            Assert.AreEqual(IntervalBoolean.True, bool3);

            bool1 = IntervalBoolean.True;
            bool2 = IntervalBoolean.False;
            bool3 = bool1 && bool2;
            Assert.AreEqual(IntervalBoolean.False, bool3);
            
            bool1 = IntervalBoolean.True;
            bool2 = IntervalBoolean.Indeterminate;
            bool3 = bool1 && bool2;
            Assert.AreEqual(IntervalBoolean.Indeterminate, bool3);
            
            //==============
            
            bool1 = IntervalBoolean.False;
            bool2 = IntervalBoolean.True;
            bool3 = bool1 && bool2;
            Assert.AreEqual(IntervalBoolean.False, bool3);
            
            bool1 = IntervalBoolean.False;
            bool2 = IntervalBoolean.False;
            bool3 = bool1 && bool2;
            Assert.AreEqual(IntervalBoolean.False, bool3);
            
            bool1 = IntervalBoolean.False;
            bool2 = IntervalBoolean.Indeterminate;
            bool3 = bool1 && bool2;
            Assert.AreEqual(IntervalBoolean.False, bool3);
            
            //==============
            
            bool1 = IntervalBoolean.Indeterminate;
            bool2 = IntervalBoolean.True;
            bool3 = bool1 && bool2;
            Assert.AreEqual(IntervalBoolean.Indeterminate, bool3);
            
            bool1 = IntervalBoolean.Indeterminate;
            bool2 = IntervalBoolean.False;
            bool3 = bool1 && bool2;
            Assert.AreEqual(IntervalBoolean.False, bool3);
            
            bool1 = IntervalBoolean.Indeterminate;
            bool2 = IntervalBoolean.Indeterminate;
            bool3 = bool1 && bool2;
            Assert.AreEqual(IntervalBoolean.Indeterminate, bool3);
        }
        
        [Test]
        public void TestOr()
        {
            var bool1 = IntervalBoolean.True;
            var bool2 = IntervalBoolean.True;
            var bool3 = bool1 || bool2;
            Assert.AreEqual(IntervalBoolean.True, bool3);

            bool1 = IntervalBoolean.True;
            bool2 = IntervalBoolean.False;
            bool3 = bool1 || bool2;
            Assert.AreEqual(IntervalBoolean.True, bool3);
            
            bool1 = IntervalBoolean.True;
            bool2 = IntervalBoolean.Indeterminate;
            bool3 = bool1 || bool2;
            Assert.AreEqual(IntervalBoolean.True, bool3);
            
            //==============
            
            bool1 = IntervalBoolean.False;
            bool2 = IntervalBoolean.True;
            bool3 = bool1 || bool2;
            Assert.AreEqual(IntervalBoolean.True, bool3);
            
            bool1 = IntervalBoolean.False;
            bool2 = IntervalBoolean.False;
            bool3 = bool1 || bool2;
            Assert.AreEqual(IntervalBoolean.False, bool3);
            
            bool1 = IntervalBoolean.False;
            bool2 = IntervalBoolean.Indeterminate;
            bool3 = bool1 || bool2;
            Assert.AreEqual(IntervalBoolean.Indeterminate, bool3);
            
            //==============
            
            bool1 = IntervalBoolean.Indeterminate;
            bool2 = IntervalBoolean.True;
            bool3 = bool1 || bool2;
            Assert.AreEqual(IntervalBoolean.True, bool3);
            
            bool1 = IntervalBoolean.Indeterminate;
            bool2 = IntervalBoolean.False;
            bool3 = bool1 || bool2;
            Assert.AreEqual(IntervalBoolean.Indeterminate, bool3);
            
            bool1 = IntervalBoolean.Indeterminate;
            bool2 = IntervalBoolean.Indeterminate;
            bool3 = bool1 || bool2;
            Assert.AreEqual(IntervalBoolean.Indeterminate, bool3);
        }

        [Test]
        public void TestNot()
        {
            Assert.AreEqual(IntervalBoolean.False, !IntervalBoolean.True);
            Assert.AreEqual(IntervalBoolean.True, !IntervalBoolean.False);
            Assert.AreEqual(IntervalBoolean.Indeterminate, !IntervalBoolean.Indeterminate);
        }

        [Test]
        public void TestXor()
        {
            var bool1 = IntervalBoolean.True;
            var bool2 = IntervalBoolean.True;
            var bool3 = bool1 ^ bool2;
            Assert.AreEqual(IntervalBoolean.False, bool3);

            bool1 = IntervalBoolean.True;
            bool2 = IntervalBoolean.False;
            bool3 = bool1 ^ bool2;
            Assert.AreEqual(IntervalBoolean.True, bool3);
            
            bool1 = IntervalBoolean.True;
            bool2 = IntervalBoolean.Indeterminate;
            bool3 = bool1 ^ bool2;
            Assert.AreEqual(IntervalBoolean.Indeterminate, bool3);
            
            //==============
            
            bool1 = IntervalBoolean.False;
            bool2 = IntervalBoolean.True;
            bool3 = bool1 ^ bool2;
            Assert.AreEqual(IntervalBoolean.True, bool3);
            
            bool1 = IntervalBoolean.False;
            bool2 = IntervalBoolean.False;
            bool3 = bool1 ^ bool2;
            Assert.AreEqual(IntervalBoolean.False, bool3);
            
            bool1 = IntervalBoolean.False;
            bool2 = IntervalBoolean.Indeterminate;
            bool3 = bool1 ^ bool2;
            Assert.AreEqual(IntervalBoolean.Indeterminate, bool3);
            
            //==============
            
            bool1 = IntervalBoolean.Indeterminate;
            bool2 = IntervalBoolean.True;
            bool3 = bool1 ^ bool2;
            Assert.AreEqual(IntervalBoolean.Indeterminate, bool3);
            
            bool1 = IntervalBoolean.Indeterminate;
            bool2 = IntervalBoolean.False;
            bool3 = bool1 ^ bool2;
            Assert.AreEqual(IntervalBoolean.Indeterminate, bool3);
            
            bool1 = IntervalBoolean.Indeterminate;
            bool2 = IntervalBoolean.Indeterminate;
            bool3 = bool1 ^ bool2;
            Assert.AreEqual(IntervalBoolean.Indeterminate, bool3);
        }

        [Test]
        public void TestFunction()
        {
            var result = Function(IntervalBoolean.False, IntervalBoolean.False, IntervalBoolean.True,
                IntervalBoolean.Indeterminate);
            Console.WriteLine(result);
        }
        
        private static IntervalBoolean Function(IntervalBoolean bool1, IntervalBoolean bool2, IntervalBoolean bool3,
            IntervalBoolean bool4)
        {
            return (bool1 && bool2) || (bool3 && bool4) ^ (bool2 && bool4);
        }
    }
}