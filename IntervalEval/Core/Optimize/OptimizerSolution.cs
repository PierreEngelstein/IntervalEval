using System.Collections.Generic;
using System.Linq;

namespace IntervalEval.Core.Optimize
{
    /// <summary>
    ///  Represents a interval box as a solution of an optimization problem.
    /// </summary>
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
        // Interval box
        public IEnumerable<Interval> Solutions { get; }
        // Gradient
        public List<Interval> Gradient { get; set; }
        // Is gradient to be used ?
        public bool GradientTagged { get; }
        // Are constraints respected ?
        public int RespectedConstraints { get; set; }
        // Which gradient has been used / solved ?
        public int GradientSolution { get; }
        // Ease access to Solutions by a direct index operator.
        public Interval this[int key] => key < Solutions.Count() ? Solutions.ToList()[key] : Interval.Entire;
    }
}