using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using IntervalEval.Core;
using IntervalEval.Core.Helpers;
using IntervalEval.Core.Optimize;

namespace IntervalEval
{
    internal class ConfigurationInterval
    {
        public double Infimum { get; set; }
        public double Supremum { get; set; }
    }
    internal class ConfigurationIntervalProbability : ConfigurationInterval
    {
        public double Probability { get; set; }
    }
    /// <summary>
    /// Configuration class for optimizer
    /// </summary>
    internal class Configuration
    {
        // Are we running the optimizer with debug printing or not ?
        public bool Debug { get; set; }
        
        // List of squared input quantum state (last double is the probability of the state)
        public List<ConfigurationIntervalProbability> Rho { get; set; }
        
        // Input ranges for variables
        public List<ConfigurationInterval> Ranges { get; set; }
        
        // How many iteration the optimizer should run ?
        public int IterationAmount { get; set; }
        public int ProblemSize { get; set; }
        
        public override string ToString()
        {
            return $"[Debug: {Debug}, Rho1: {Rho}, IterationAmount: {IterationAmount}]";
        }
    }

    internal static class Program
    {
        static void Main(string[] args)
        {
            // Get configuration from file
            var fileContent = File.ReadAllText("configuration.json");
            Console.WriteLine(fileContent);
            var configuration = JsonSerializer.Deserialize<Configuration>(fileContent);
            if(configuration == null) return;
            Console.WriteLine(configuration);

            // Setup problem input
            var problem = ProblemDescriptor.Problems3D[1];
            var listInputMi = new List<object>();
            listInputMi.AddRange(configuration.Rho.Select(rho => new Tuple<double, double>(rho.Infimum, rho.Supremum)));
            listInputMi.Add(new Tuple<double, double>(configuration.Rho[0].Probability, configuration.Rho[1].Probability));
            var fMin = 0.0;
            var fMax = 0.0;
            var fPrecision = 0.0;

            // Optimizer callbacks
            Optimizer.PrecisionF.OnChange += (_, d) =>
            {
                if(double.IsNaN(d)) return;
                fPrecision = d;
            };
            Optimizer.IntervalFMinimum.OnChange += (_, d) =>
            {
                if(double.IsNaN(d)) return;
                fMin = d;
                Console.WriteLine($"fMin = {fMin}");
            };
            Optimizer.IntervalFMaximum.OnChange += (_, d) =>
            {
                if(double.IsNaN(d)) return;
                fMax = d;
                Console.WriteLine($"fMax = {fMax}");
            };
            Optimizer.EvolutionBoxesAmount.OnChange += (_, boxes) =>
            {
                if (!boxes.Any()) return;
                Console.WriteLine($"{boxes.Count - 1} => {boxes[^1]}");
                var proc = Process.GetCurrentProcess();
                var mem = proc.PrivateMemorySize64 / (1024.0*1024.0);
                Console.WriteLine($"RAM usage: {mem} MB");
                Console.WriteLine("===================");
            };

            // Optimize (and measure execution time)
            var sw = new Stopwatch();
            sw.Start();
            var result = Optimizer.Optimize(new[]
            {
                Interval.FromInfSup(configuration.Ranges[0].Infimum, configuration.Ranges[0].Supremum),
                Interval.FromInfSup(configuration.Ranges[1].Infimum, configuration.Ranges[1].Supremum),
                Interval.FromInfSup(configuration.Ranges[2].Infimum, configuration.Ranges[2].Supremum),
            }, problem.Function, problem.Constraints, OptimizationType.Maximization, configuration.IterationAmount, configuration.Debug, listInputMi);
            sw.Stop();
            Console.WriteLine($"Optimization took {sw.ElapsedMilliseconds} ms");
            // Get minimal / maximal values of each variable
            var optimizerSolutions = result as OptimizerSolution[] ?? result.ToArray();

            var variablesBounds = new List<Tuple<double, double>>();
            for (var i = 0; i < configuration.ProblemSize; i++)
            {
                var tuple = new Tuple<double, double>(optimizerSolutions[0][i].Infimum, optimizerSolutions[0][i].Supremum);
                variablesBounds.Add(tuple);
            }

            foreach (var optimizerSolution in optimizerSolutions)
            {
                for (var i = 0; i < configuration.ProblemSize; i++)
                {
                    if (optimizerSolution[i].Infimum <= variablesBounds[i].Item1) variablesBounds[i] = new Tuple<double, double>(optimizerSolution[i].Infimum, variablesBounds[i].Item2);
                    if (optimizerSolution[i].Supremum >= variablesBounds[i].Item2) variablesBounds[i] = new Tuple<double, double>(variablesBounds[i].Item1, optimizerSolution[i].Supremum);
                }
            }

            // Print result
            Console.WriteLine($"F in <{fMin}, {fMax}>");
            Console.WriteLine($"Absolute precision on F: {fPrecision}");
            Console.WriteLine($"Solution in [<{variablesBounds[0].Item1}, {variablesBounds[0].Item2}>, <{variablesBounds[1].Item1}, {variablesBounds[1].Item2}>, <{variablesBounds[2].Item1}, {variablesBounds[2].Item2}>]");
        }
    }
}