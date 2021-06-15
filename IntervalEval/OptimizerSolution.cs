using System.Collections.Generic;
using System.Linq;
using IntSharp.Types;

namespace IntervalEval
{
    public class OptimizerSolution
    {
        public OptimizerSolution(IEnumerable<Interval> solutions, bool gradientTagged, int gradientSolution, IEnumerable<Interval> gradient, int respectedConstraints = 0)
        {
            Solutions = solutions;
            GradientTagged = gradientTagged;
            GradientSolution = gradientSolution;
            Gradient = gradient?.ToList();
            RespectedConstraints = respectedConstraints;
        }
        
        public IEnumerable<Interval> Solutions { get; }
        
        public List<Interval> Gradient { get; set; }
        public bool GradientTagged { get; }
        
        public int RespectedConstraints { get; set; }
        public int GradientSolution { get; }

        public Interval this[int key] => key < Solutions.Count() ? Solutions.ToList()[key] : Interval.Entire;
    }
}