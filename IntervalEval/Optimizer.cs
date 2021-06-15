using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IntSharp;
using IntSharp.Types;
using Math = System.Math;

namespace IntervalEval
{
    public enum OptimizationType
    {
        Minimization = 0,
        Maximization = 1
    }
    
    public static class Optimizer
    {
        public static readonly TypeListener<int> OptimizationIterations = new();
        public static readonly TypeListener<double> PrecisionF = new();
        public static readonly TypeListener<double> IntervalFMinimum = new();
        public static readonly TypeListener<double> IntervalFMaximum = new();
        public static readonly TypeListener<List<double>> EvolutionBoxesAmount = new();
        public static readonly TypeListener<List<Dictionary<int, double>>> EvolutionVolumeBoxesByCategory = new();
        private const int MaxListCount = 100000;

        /// <summary>
        /// Launches an optimization operation.
        /// </summary>
        /// <param name="variables">List of variables as input of the problem. Define them as initial interval for the problem</param>
        /// <param name="function">The function to be optimized</param>
        /// <param name="constraints">Constraint function applied to variables</param>
        /// <param name="optimizationType">Are we maximizing or minimizing ? Defaults to minimization</param>
        /// <param name="iterations">Number of iterations for the optimization. Defaults to 1</param>
        /// <param name="debug">If true, prints additional debug information to console.</param>
        /// <param name="additionalArguments">Additional arguments passed to the evaluation function if needed</param>
        /// <param name="token">Cancels the running optimization if asked.</param>
        /// <returns>List of reduced intervals as solution</returns>
        public static IEnumerable<OptimizerSolution> Optimize(
            IEnumerable<Interval> variables, 
            Func<OptimizerSolution, List<object>, Tuple<Interval, bool, int, IEnumerable<Interval>>> function,
            Func<OptimizerSolution, bool> constraints,
            OptimizationType optimizationType = OptimizationType.Minimization, 
            int iterations = 1,
            bool debug = false,
            List<object> additionalArguments = null,
            CancellationToken token = default
            )
        {
            EvolutionBoxesAmount.Value = new List<double>();
            EvolutionVolumeBoxesByCategory.Value = new List<Dictionary<int, double>>();
            if(debug) Console.WriteLine(optimizationType == OptimizationType.Minimization ? "Min" : "Max");
            var enumerable = variables as Interval[] ?? variables.ToArray();
            // List of current leaves to be processed
            var currentList = new List<OptimizerSolution> {new(enumerable, false, -1, null)};
            var sw = new Stopwatch();
            for (OptimizationIterations.Value = 0; OptimizationIterations.Value <= iterations ; OptimizationIterations.Value++)
            {
                if (token.IsCancellationRequested) return new List<OptimizerSolution>(); // Cancel operation if asked
                if(debug) Console.WriteLine($"Iteration {OptimizationIterations.Value}");
                // New list of leaves
                var nextList = new List<OptimizerSolution>();
                // List of leaves that are uncertain (ex: have thrown an IntervalEvalException, meaning we don't treat them and just bisect and treat their children
                var uncertainList = new List<OptimizerSolution>();
                var listImages = new List<Interval>();
                var listImagesMedium = new List<Interval>();
                
                //=============================
                // Process each leaf in the list (parallelized, gains a lot of time) 
                //=============================
                var tokenCancelled = false;
                sw.Restart();
                Parallel.ForEach(currentList, (intervals, state) =>
                {
                    if (token.IsCancellationRequested)
                    {
                        tokenCancelled = true;
                        state.Break();
                    }
                    // Bisect the leaf, and check constraints. If the resulting intervals matches constraints,
                    // add them to the new list of leaves, otherwise throw them out.
                    var (leftIt, rightIt) = intervals.Bisect();
                    if (constraints(leftIt))
                    {
                        // Compute image of left bisected box
                        try
                        {
                            var (interval, isGradient, gradientSolution, gradient) = function(leftIt, additionalArguments);
                            if (!interval.IsEmpty())
                            {
                                var (middle, _, _, _) = function(leftIt.Middle(), additionalArguments);
                                lock (nextList) lock(listImages) lock(listImagesMedium)
                                {
                                    nextList.Add(new OptimizerSolution(leftIt.Solutions, isGradient, gradientSolution, gradient, leftIt.RespectedConstraints));
                                    listImages.Add(interval);
                                    listImagesMedium.Add(middle);
                                }
                            }
                            else if(debug)
                            {
                                Console.WriteLine("Empty left box.");
                            }
                        }
                        catch (IntervalEvalException e)
                        {
                            if(debug) Console.WriteLine("Uncertain left box.");
                            uncertainList.Add(new OptimizerSolution(leftIt.Solutions, leftIt.GradientTagged, leftIt.GradientSolution, leftIt.Gradient, leftIt.RespectedConstraints));
                        }
                    }
                    else
                    {
                        if(debug) Console.Write("Removing out-constrained interval.");
                        if(debug) IntervalHelpers.Print(leftIt.Solutions);
                    }
                    
                    if (constraints(rightIt))
                    {
                        // Compute image of right bisected box
                        try
                        {
                            var (interval, isGradient, gradientSolution, gradient) = function(rightIt, additionalArguments);
                            if (!interval.IsEmpty())
                            {
                                var (middle, _, _, _) = function(rightIt.Middle(), additionalArguments);
                                lock (nextList) lock(listImages) lock(listImagesMedium)
                                {
                                    nextList.Add(new OptimizerSolution(rightIt.Solutions, isGradient, gradientSolution, gradient, rightIt.RespectedConstraints));
                                    listImages.Add(interval);
                                    listImagesMedium.Add(middle);
                                }
                            }
                            else if(debug)
                            {
                                Console.WriteLine("Empty right box.");
                            }
                        }
                        catch (IntervalEvalException e)
                        {
                            if(debug) Console.WriteLine("Uncertain right box.");
                            uncertainList.Add(new OptimizerSolution(rightIt.Solutions, rightIt.GradientTagged, rightIt.GradientSolution, rightIt.Gradient, rightIt.RespectedConstraints));
                        }
                    }
                    else
                    {
                        if(debug) Console.Write("Removing out-constrained interval.");
                        if(debug) IntervalHelpers.Print(rightIt.Solutions);
                    }
                });
                sw.Stop();
                if(tokenCancelled) return new List<OptimizerSolution>(); // Cancel operation if asked
                Console.WriteLine($"Leaf Processing: {sw.ElapsedMilliseconds} ms");
                
                var correctList = new List<OptimizerSolution>();
                if(listImages.Count != 0)
                {
                    IntervalFMinimum.Value = listImages[0].Infimum;
                    IntervalFMaximum.Value = listImages[0].Supremum;
                }
                else
                {
                    IntervalFMinimum.Value = IntervalFMaximum.Value = 0;
                }
                
                //=============================
                // Remove out of bounds leaves
                //=============================
                
                // Get highest middle point
                // Corresponds to sup([f]([x]) <= f(a) => x* not solution.
                // We choose f(a) as the highest f([x].mid) (for max, or lowest for min)
                sw.Restart();
                var indexBestMiddle = 0;
                for (var i = 0; i < nextList.Count; i++)
                {
                    switch (optimizationType)
                    {
                        case OptimizationType.Minimization:
                            if (listImagesMedium[i].Mid() < listImagesMedium[indexBestMiddle])
                                indexBestMiddle = i;
                            break;
                        case OptimizationType.Maximization:
                            if (listImagesMedium[i].Mid() > listImagesMedium[indexBestMiddle])
                                indexBestMiddle = i;
                            break;
                        default:
                            if(debug) Console.WriteLine($"Warning: incorrect optimization type {optimizationType}");
                            break;
                    }
                }

                for (var i = 0; i < nextList.Count; i++)
                {
                    switch (optimizationType)
                    {
                        case OptimizationType.Minimization:
                            if(!(listImages[i].Infimum > listImagesMedium[indexBestMiddle])) correctList.Add(nextList[i]);
                            break;
                        case OptimizationType.Maximization:
                            if(!(listImages[i].Supremum < listImagesMedium[indexBestMiddle])) correctList.Add(nextList[i]);
                            break;
                        default:
                            if(debug) Console.WriteLine($"Warning: incorrect optimization type {optimizationType}");
                            break;
                    }
                }
                sw.Stop();
                if(debug) Console.WriteLine($"Removed {nextList.Count - correctList.Count} / {nextList.Count} boxes in {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"Out of bounds removal: {sw.ElapsedMilliseconds} ms");
                // PrecisionF.Value = Math.Abs(IntervalFMaximum.Value - IntervalFMinimum.Value);
                PrecisionF.Value = Math.Abs(IntervalFMaximum.Value - IntervalFMinimum.Value) / IntervalFMaximum.Value;
                // Assign new list of leaves to the current one, and follow with new processing
                currentList.Clear();
                currentList = correctList;
                currentList.AddRange(uncertainList);
                Console.WriteLine($"correct: {correctList.Count} ; uncertain: {uncertainList.Count}");

                var currentEvolutionBoxesAmount = EvolutionBoxesAmount.Value;
                currentEvolutionBoxesAmount.Add(currentList.Count + uncertainList.Count);
                EvolutionBoxesAmount.Value = currentEvolutionBoxesAmount;
                var currentEvolutionVolume = EvolutionVolumeBoxesByCategory.Value;
                var dictionnary = new Dictionary<int, double>();
                foreach (var optimizerSolution in currentList)
                {
                    if(optimizerSolution == null) continue;
                    if (dictionnary.ContainsKey(optimizerSolution.RespectedConstraints))
                    {
                        // dictionnary[optimizerSolution.RespectedConstraints] += optimizerSolution.Volume();
                        dictionnary[optimizerSolution.RespectedConstraints] += 1;
                    }
                    else
                    {
                        // dictionnary.Add(optimizerSolution.RespectedConstraints, optimizerSolution.Volume());
                        dictionnary.Add(optimizerSolution.RespectedConstraints, 1);
                    }
                        
                }
                currentEvolutionVolume.Add(dictionnary);
                EvolutionVolumeBoxesByCategory.Value = currentEvolutionVolume;

                if (!debug) continue;
                {
                    foreach (var intervals in currentList)
                    {
                        IntervalHelpers.Print(intervals.Solutions);
                    }
                    Console.WriteLine("==========");
                }
            }

            return currentList;
        }
    }
}