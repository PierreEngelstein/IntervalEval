using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IntervalEval.Core.MutualInformationProblem;
using IntervalEval.Core.Optimize;

namespace IntervalEval.Core.Helpers
{
    /// <summary>
    /// Provides a way to store in objects problems to be solved.
    /// </summary>
    public class ProblemDescriptor
    {
        private static IEnumerable<string> _lockingPrintGradient = new string[1];
        
        // Mathematical representation of the cost function
        private string ProblemFunctionDescription { get; init; }
        // Mathematical representation of the constraints
        private string ProblemConstraintsDescription { get; init; }
        
        /// <summary>
        /// Cost function evaluated over a OptimizerSolution (a box)
        /// </summary>
        /// <param>OptimizerSolution: function box input</param>
        /// <param>List(object): List of additional arguments that may be needed for computation.</param>
        /// <param>Tuple(Interval, bool, int, IEnumerable(Interval)): Return type of the function</param>
        ///     -> <param>Interval</param>: the evaluation of the function (We consider only f : R^n -> R^1)
        ///     -> <param>bool</param>: do we want to use gradient for evaluation. (default: false) 
        ///     -> <param>int</param>: which gradient was satisfied ? (default: -1)
        ///     -> <param>IEnumerable(Interval)</param>: result of the gradient (default: null)
        public Func<OptimizerSolution, List<object>, Tuple<Interval, bool, int, IEnumerable<Interval>>> Function { get; private init; }
        /// <summary>
        /// Constraint function over a box. Returns true if the box satisfies the constraints, false otherwise.
        /// </summary>
        public Func<OptimizerSolution, bool> Constraints { get; private init; }

        public override string ToString()
        {
            return
                $"{ProblemFunctionDescription}{(string.IsNullOrEmpty(ProblemConstraintsDescription) ? "" : $" ; {ProblemConstraintsDescription}")}";
        }
        
        private static ProblemDescriptor ProblemX1X2 => new()
        {
            ProblemFunctionDescription = "f(x1, x2) = x1 + x2",
            ProblemConstraintsDescription = "x1² + x2² = 1",
            Function = (variables, _) =>
            {
                var listVars = variables.Solutions.ToList();
                return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.Zero, false, -1, null);
                // return new Tuple<Interval, bool, int, IEnumerable<Interval>>(listVars.Count != 2 ? Interval.FromInfSup(double.NaN, double.NaN) : listVars[0] + listVars[1], false, -1, null);
            },
            Constraints = variables =>
            {
                return true;
                // var enumerable = variables.Solutions as Interval[] ?? variables.Solutions.ToArray();
                // if (enumerable.Length != 2)
                // {
                //     return false;
                // }
                // // z = x1² + x2² - 1
                // var listVars = enumerable.ToList();
                // var result1 = IntSharp.Math.Sqr(listVars[0]) + IntSharp.Math.Sqr(listVars[1]) - 1.0;
                // // z = x1 - x2
                // var result2 = listVars[0] - listVars[1];
                // return result1.ContainsZero() && result2.ContainsZero();
            }
        };
        
        private static ProblemDescriptor ProblemX1X2Ineq => new()
        {
            ProblemFunctionDescription = "f(x1, x2) = x1 + x2 (v2)",
            ProblemConstraintsDescription = "x1² + x2² - 1 < 0",
            Function = (variables, _) =>
            {
                var listVars = variables.Solutions.ToList();
                return new Tuple<Interval, bool, int, IEnumerable<Interval>>(listVars.Count != 2 ? Interval.FromInfSup(double.NaN, double.NaN) : listVars[0] + listVars[1], false, -1, null);
            },
            Constraints = variables =>
            {
                var enumerable = variables.Solutions as Interval[] ?? variables.Solutions.ToArray();
                if (enumerable.Length != 2)
                {
                    return false;
                }
                // x1² + x2² - 1 < 0
                var listVars = enumerable.ToList();
                var result1 = Core.IntervalMath.Sqr(listVars[0]) + Core.IntervalMath.Sqr(listVars[1]) - 1.0;
                if (result1.Supremum < 0)
                {
                    // Test gradient a l'interieur: si contient 0 => renvoie faux
                    // Ici, grad(g) = (1, 1), c'est donc faux
                    return false;
                }
                if (result1.ContainsZero())
                {
                    // si sur le bord, vérifie grad(f) grad(g) contient 0
                    var result2 = listVars[0] - listVars[1];
                    return result2.ContainsZero();
                }
                return false;
            }
        };
        
        private static ProblemDescriptor ProblemX1X2Ineq2 => new()
        {
            ProblemFunctionDescription = "f(x1, x2) = x1 + x2 (v3)",
            ProblemConstraintsDescription = "x1² + x2² - 1 < 0",
            Function = (variables, _) =>
            {
                var listVars = variables.Solutions.ToList();
                return new Tuple<Interval, bool, int, IEnumerable<Interval>>(listVars.Count != 2 ? Interval.FromInfSup(double.NaN, double.NaN) : listVars[0] + listVars[1], false, -1, null);
            },
            Constraints = variables =>
            {
                var enumerable = variables.Solutions as Interval[] ?? variables.Solutions.ToArray();
                if (enumerable.Length != 2)
                {
                    return false;
                }
                // x1² + x2² - 1 < 0
                var listVars = enumerable.ToList();
                var result1 = Core.IntervalMath.Sqr(listVars[0]) + Core.IntervalMath.Sqr(listVars[1]) - 1.0;
                if (result1.Supremum < 0)
                {
                    // Test gradient a l'interieur: si contient 0 => renvoie faux
                    // Ici, grad(g) = (1, 1), c'est donc faux
                    return false;
                }
                if (result1.ContainsZero())
                {
                    // si sur le bord, vérifie grad(f) grad(g) contient 0
                    var result2 = listVars[0] - listVars[1];
                    return result2.ContainsZero();
                }
                return false;
            }
        };
        
        private static ProblemDescriptor ProblemX1X2Ineq3 => new()
        {
            ProblemFunctionDescription = "f(x1, x2) = x1 + x2 (v4)",
            ProblemConstraintsDescription = "x1² + x2² - 1 < 0",
            Function = (variables, _) =>
            {
                var listVars = variables.Solutions.ToList();
                return new Tuple<Interval, bool, int, IEnumerable<Interval>>(listVars.Count != 2 ? Interval.FromInfSup(double.NaN, double.NaN) : Interval.Zero, false, -1, null);
            },
            Constraints = variables =>
            {
                var enumerable = variables.Solutions as Interval[] ?? variables.Solutions.ToArray();
                if (enumerable.Length != 2)
                {
                    return false;
                }
                // x1² + x2² - 1 < 0
                var listVars = enumerable.ToList();
                var ctr1 = Core.IntervalMath.Sqr(listVars[0]) + Core.IntervalMath.Sqr(listVars[1]) - 1.0;
                var ctr2 = -listVars[0] - listVars[1];
                var toKeep = true;
                
                var ctr1Contains = ctr1.ContainsZero();
                var ctr2Contains = ctr2.ContainsZero();
                var ctr1Inside = ctr1.Supremum <= 0;
                var ctr2Inside = ctr2.Supremum <= 0;
                var ctr1Outside = ctr1.Infimum > 0;
                var ctr2Outside = ctr2.Infimum > 0;
                
                if (ctr1Inside && ctr2Inside)
                    toKeep = false; // red boxes

                if (ctr1Outside || ctr2Outside)
                    toKeep = false; // Blue boxes
                
                return toKeep;
            }
        };
        
        private static ProblemDescriptor ProblemX1X2Ineq4 => new()
        {
            ProblemFunctionDescription = "f(x1, x2) = x1 + x2 (v5)",
            ProblemConstraintsDescription = "x1² + x2² - 1 < 0",
            Function = (variables, _) =>
            {
                var listVars = variables.Solutions.ToList();
                return new Tuple<Interval, bool, int, IEnumerable<Interval>>(listVars.Count != 2 ? Interval.FromInfSup(double.NaN, double.NaN) : Interval.Zero, false, -1, null);
            },
            Constraints = variables =>
            {
                var enumerable = variables.Solutions as Interval[] ?? variables.Solutions.ToArray();
                if (enumerable.Length != 2)
                {
                    return false;
                }
                // x1² + x2² - 1 < 0
                var listVars = enumerable.ToList();
                var ctr1 = listVars[0] - 1.5;
                var ctr2 = -listVars[0] + 0.7;
                var ctr3 = listVars[1] - 1;
                var ctr4 = -listVars[1] + 0.5;
                var toKeep = true;
                var ctr1Contains = ctr1.ContainsZero();
                var ctr2Contains = ctr2.ContainsZero();
                var ctr3Contains = ctr3.ContainsZero();
                var ctr4Contains = ctr4.ContainsZero();

                var ctr1Inside = ctr1.Supremum <= 0;
                var ctr2Inside = ctr2.Supremum <= 0;
                var ctr3Inside = ctr3.Supremum <= 0;
                var ctr4Inside = ctr4.Supremum <= 0;

                var ctr1Outside = ctr1.Infimum > 0;
                var ctr2Outside = ctr2.Infimum > 0;
                var ctr3Outside = ctr3.Infimum > 0;
                var ctr4Outside = ctr4.Infimum > 0;

                if (ctr1Inside && ctr2Inside && ctr3Inside && ctr4Inside)
                    toKeep = false; // red boxes

                if (ctr1Outside || ctr2Outside || ctr3Outside || ctr4Outside)
                    toKeep = false; // Blue boxes

                if (!toKeep)
                {
                    // TODO : function gradient // constraint gradient
                    if (ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains)
                    {
                    }else if (!ctr1Contains && ctr2Contains && !ctr3Contains && !ctr4Contains)
                    {
                    }else if (!ctr1Contains && !ctr2Contains && ctr3Contains && !ctr4Contains)
                    {
                    }else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && ctr4Contains)
                    {
                    }
                }

                return toKeep;
            }
        };
        
        private static ProblemDescriptor ProblemX1X2Square => new()
        {
            ProblemFunctionDescription = "f(x1, x2) = (x1 + x2)²",
            ProblemConstraintsDescription = "x1² + x2² = 1",
            Function = (variables, _) =>
            {
                var listVars = variables.Solutions.ToList();
                return new Tuple<Interval, bool, int, IEnumerable<Interval>>(listVars.Count != 2 ? Interval.FromInfSup(double.NaN, double.NaN) : Core.IntervalMath.Sqr(listVars[0] + listVars[1]), false, -1, null);
            },
            Constraints = variables =>
            {
                var enumerable = variables.Solutions as Interval[] ?? variables.Solutions.ToArray();
                if (enumerable.Length != 2)
                {
                    return false;
                }
                var listVars = enumerable.ToList();
                var ctr1 = Core.IntervalMath.Sqr(listVars[0]) + Core.IntervalMath.Sqr(listVars[1]) - 1.0; // Constraint
                var ctr2 = listVars[0] - listVars[1]; //gradient(f) = \alpha gradient(g) => KKT
                return ctr1.ContainsZero() && ctr2.ContainsZero();
            }
        };
        
        private static ProblemDescriptor ProblemX1SqrX2 => new()
        {
            ProblemFunctionDescription = "f(x1, x2) = x1²x2",
            ProblemConstraintsDescription = "x1² + x2² = 3",
            Function = (variables, _) =>
            {
                var listVars = variables.Solutions.ToList();
                return new Tuple<Interval, bool, int, IEnumerable<Interval>>(listVars.Count != 2 ? Interval.FromInfSup(double.NaN, double.NaN) : Core.IntervalMath.Sqr(listVars[0]) * listVars[1], false, -1, null);
            },
            Constraints = variables =>
            {
                var enumerable = variables.Solutions as Interval[] ?? variables.Solutions.ToArray();
                if (enumerable.Length != 2)
                {
                    return false;
                }
                var listVars = enumerable.ToList();
                var ctr1 = Core.IntervalMath.Sqr(listVars[0]) + Core.IntervalMath.Sqr(listVars[1]) - 3.0;
                var ctr2 = Core.IntervalMath.Sqr(listVars[0]) - Core.IntervalMath.Sqr(listVars[1]); //gradient(f) = \alpha gradient(g) => KKT
                return ctr1.ContainsZero() && ctr2.ContainsZero();
            }
        };
        private static ProblemDescriptor ProblemXLogX1 => new()
        {
            ProblemFunctionDescription = "f(x1, x2) = x1*log(x1) + x2 * log(x2)",
            ProblemConstraintsDescription = "x1² + x2² = 3",
            Function = (variables, _) =>
            {
                var listVars = variables.Solutions.ToList();
                return new Tuple<Interval, bool, int, IEnumerable<Interval>>(listVars.Count != 2 ? Interval.FromInfSup(double.NaN, double.NaN) : IntervalMath.XLog(listVars[0], "none") + IntervalMath.XLog(listVars[1], "none"), false, -1, null);
            },
            Constraints = variables =>
            {
                var enumerable = variables.Solutions as Interval[] ?? variables.Solutions.ToArray();
                if (enumerable.Length != 2)
                {
                    return false;
                }
                var listVars = enumerable.ToList();
                var ctr1 = Core.IntervalMath.Sqr(listVars[0]) + Core.IntervalMath.Sqr(listVars[1]) - 3.0; 
                return ctr1.ContainsZero();
            }
        };
        
        private static ProblemDescriptor ProblemXLogX2 => new()
        {
            ProblemFunctionDescription = "f(x1, x2) = (x1 + x2) * log(x1 + x2) + (x1 - x2) * log(x1 - x2)",
            Function = (variables, _) =>
            {
                var listVars = variables.Solutions.ToList();
                return new Tuple<Interval, bool, int, IEnumerable<Interval>>(listVars.Count != 2 ? Interval.FromInfSup(double.NaN, double.NaN) : IntervalMath.XLog(listVars[0] + listVars[1], "none") + IntervalMath.XLog(listVars[0] - listVars[1], "none"), false, -1, null);
            },
            Constraints = _ => true
        };
        
        private static ProblemDescriptor ProblemXLogX3 => new()
        {
            ProblemFunctionDescription = "f(x1, x2, x3) = (x1 + x2) * log(x1 + x2) + (x1 - x3) * log(x1 - x3)",
            Function = (variables, _) =>
            {
                var listVars = variables.Solutions.ToList();
                return new Tuple<Interval, bool, int, IEnumerable<Interval>>(listVars.Count != 3 ? Interval.FromInfSup(double.NaN, double.NaN) : IntervalMath.XLog(listVars[0] + listVars[1], "none") + IntervalMath.XLog(listVars[0] - listVars[2], "none"), false, -1, null);
            },
            Constraints = _ => true
        };

        private static ProblemDescriptor ProblemMutualInformation => new()
        {
            ProblemFunctionDescription = "Mutual Information",
            ProblemConstraintsDescription = "",
            Function = (variables, optionalArguments) =>
            {
                if (optionalArguments.Count != 3)
                    return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null);
                if(!(optionalArguments[0] is Tuple<double, double>)) return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null);
                if(!(optionalArguments[1] is Tuple<double, double>)) return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null);
                
                var (p11Squared, p12Squared) = (Tuple<double, double>) optionalArguments[0];
                var (p21Squared, p22Squared) = (Tuple<double, double>) optionalArguments[1];
                var (probabilityP1, probabilityP2) = (Tuple<double, double>) optionalArguments[2];

                const double p1 = 0.1;
                const double p2 = 0.9;
                
                var y1A = p11Squared * p1;
                var y1B = Math.Sqrt(p11Squared * p12Squared) * p1;
                var y1C = p12Squared * p1;
                
                var y2A = p21Squared * p2;
                var y2B = Math.Sqrt(p21Squared * p22Squared) * p2;
                var y2C = p22Squared * p2;
                
                var listVars = variables.Solutions.ToList();
                
                var aPivot = listVars[0].Infimum;
                var bPivot = listVars[1].Infimum;
                var dPivot = listVars[2].Infimum;
                for (var k = 0; k < 8; k++)
                {
                    var bitArray = new BitArray(new[] {k});
                    aPivot = bitArray[0] ? listVars[0].Infimum : listVars[0].Supremum;
                    bPivot = bitArray[0] ? listVars[0].Infimum : listVars[0].Supremum;
                    dPivot = bitArray[0] ? listVars[0].Infimum : listVars[0].Supremum;
                    var ctr1 = aPivot * dPivot - bPivot * bPivot;
                    var ctr2 = (1-aPivot) * (1-dPivot) - (-bPivot) * (-bPivot);
                    if (ctr1 >= 0 && ctr2 >= 0) break;
                }

                
                
                return listVars.Count != 3
                    ? new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.Empty, false, -1, null)
                    : MutualInfoHelpers2Variables.MutualInformation(
                        listVars[0], listVars[1], listVars[2],
                        y1A, y1B, y1C, y2A, y2B, y2C, probabilityP1, probabilityP2, aPivot, bPivot, dPivot
                        , variables.GradientTagged, variables.GradientSolution, variables.Gradient);
            },
            Constraints = variables =>
            {
                var enumerable = variables.Solutions as Interval[] ?? variables.Solutions.ToArray();
                if (enumerable.Length != 3)
                {
                    return false;
                }

                var listVars = enumerable.ToList();
                // Bounds
                var toKeep = true;
                
                var ctr1 = -listVars[0];
                var ctr2 =  listVars[0] - 0.5;
                var ctr3 = -listVars[1] - 1;
                var ctr4 =  listVars[1] - 1;
                var ctr5 = -listVars[2];
                var ctr6 = listVars[2] - 1;
                // Positive semi-definite
                var ctr7 = -listVars[0] * listVars[2] + Core.IntervalMath.Sqr(listVars[1]);
                var ctr8 = -(1 - listVars[0]) * (1 - listVars[2]) + Core.IntervalMath.Sqr(- listVars[1]);
                // Purity one
                var ctr9 = (listVars[0] + listVars[2] - 1);

                var ctrContains = new List<bool>
                {
                    ctr1.ContainsZero(),
                    ctr2.ContainsZero(),
                    ctr3.ContainsZero(),
                    ctr4.ContainsZero(),
                    ctr5.ContainsZero(),
                    ctr6.ContainsZero(),
                    ctr7.ContainsZero(),
                    ctr8.ContainsZero(),
                    // ctr9.ContainsZero(),
                };
                var ctr1Contains = ctr1.ContainsZero();
                var ctr2Contains = ctr2.ContainsZero();
                var ctr3Contains = ctr3.ContainsZero();
                var ctr4Contains = ctr4.ContainsZero();
                var ctr5Contains = ctr5.ContainsZero();
                var ctr6Contains = ctr6.ContainsZero();
                var ctr7Contains = ctr7.ContainsZero();
                var ctr8Contains = ctr8.ContainsZero();
                // var ctr9Contains = ctr9.ContainsZero();

                var ctr1Inside = ctr1.Supremum <= 0;
                var ctr2Inside = ctr2.Supremum <= 0;
                var ctr3Inside = ctr3.Supremum <= 0;
                var ctr4Inside = ctr4.Supremum <= 0;
                var ctr5Inside = ctr5.Supremum <= 0;
                var ctr6Inside = ctr6.Supremum <= 0;
                var ctr7Inside = ctr7.Supremum <= 0;
                var ctr8Inside = ctr8.Supremum <= 0;

                var ctr1Outside = ctr1.Infimum > 0;
                var ctr2Outside = ctr2.Infimum > 0;
                var ctr3Outside = ctr3.Infimum > 0;
                var ctr4Outside = ctr4.Infimum > 0;
                var ctr5Outside = ctr5.Infimum > 0;
                var ctr6Outside = ctr6.Infimum > 0;
                var ctr7Outside = ctr7.Infimum > 0;
                var ctr8Outside = ctr8.Infimum > 0;

                // (0, 0, 0) not in [df]([x]) and [x] completely inside admissible space => x* not solution
                if (ctr1Inside && ctr2Inside && ctr3Inside && ctr4Inside && ctr5Inside && ctr6Inside && ctr7Inside && ctr8Inside)
                    toKeep = false; // red boxes

                // [x] outside of admissible space => x* not solution
                if (ctr1Outside || ctr2Outside || ctr3Outside || ctr4Outside || ctr5Outside || ctr6Outside  || ctr7Outside || ctr8Outside)
                    toKeep = false; // Blue boxes

                // 0 in g_j([x]) AND forall(i != j) 0 not in g_i([x]) AND dg_j // df AND 0 not in [df]([x]) => x* not solution
                // lock (_lockingPrintGradient)
                // {
                // Only one constraint verified
                var countConstraints = 0;
                var indexLastConstraint = 0;
                for (var i = 0; i < ctrContains.Count; i++)
                {
                    if (!ctrContains[i]) continue;
                    countConstraints++;
                    indexLastConstraint = i;
                }
                
                // Are grad(g) and grad(f) independent ?
                var indep = false;
                if (countConstraints == 1 && variables.Gradient != null)
                {
                    switch (indexLastConstraint)
                    {
                        // lock (_lockingPrintGradient)
                        // {
                        //     Console.WriteLine($"SINGLE CONSTRAINT (ctr{indexLastConstraint})!!!");
                        // }
                        case 0:
                        case 1:
                        {
                            var bool1 = variables.Gradient[1].ContainsZero();
                            var bool2 = variables.Gradient[2].ContainsZero();
                            indep = bool1 && bool2;
                            break;
                        }
                        case 2:
                        case 3:
                        {
                            var bool1 = variables.Gradient[0].ContainsZero();
                            var bool2 = variables.Gradient[2].ContainsZero();
                            indep = bool1 && bool2;
                            break;
                        }
                        case 4:
                        case 5:
                        {
                            var bool1 = variables.Gradient[0].ContainsZero();
                            var bool2 = variables.Gradient[1].ContainsZero();
                            indep = bool1 && bool2;
                            break;
                        }
                        case 6:
                        case 7:
                        {
                            var bool1 = variables.Gradient[0];
                            break;
                        }
                    }
                }
                if (indep)
                {
                    // lock (_lockingPrintGradient)
                    // {
                    //     Console.WriteLine("INDEP !!!");
                    // }
                    toKeep = !variables.GradientTagged;
                    // if (!toKeep) toKeep = false;
                }
                // }
                // if (!ctr9Contains) toKeep = false;
                
                if(!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 1;
                else if ( ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 2;
                else if (!ctr1Contains &&  ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 3;  //<== green
                else if (!ctr1Contains && !ctr2Contains &&  ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 4;
                else if (!ctr1Contains && !ctr2Contains && !ctr3Contains &&  ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 5;
                else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains &&  ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 6;
                else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains &&  ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 7;
                else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains &&  ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 8;  //<== white eb
                else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains &&  ctr8Contains) variables.RespectedConstraints = 9;  //<== yellow
                else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains &&  ctr7Contains &&  ctr8Contains) variables.RespectedConstraints = 10; //<== blue
                else if ( ctr1Contains &&  ctr2Contains &&  ctr3Contains &&  ctr4Contains &&  ctr5Contains &&  ctr6Contains &&  ctr7Contains &&  ctr8Contains) variables.RespectedConstraints = 11; //<== gray 7f
                else variables.RespectedConstraints = 12; //<== pink 2

                return toKeep;
            }
        };
        
        private static ProblemDescriptor ProblemMI3 => new ()
        {
            ProblemFunctionDescription = "Mutual Information with 3 variables",
            ProblemConstraintsDescription = "",
            Function = (variables, optionalArguments) =>
            {
                if (optionalArguments.Count != 3) return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null);
                if(!(optionalArguments[0] is Tuple<double, double>)) return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null);
                if(!(optionalArguments[1] is Tuple<double, double>)) return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null);
                if(!(optionalArguments[2] is Tuple<double, double>)) return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null);
                var (item1, item2) = (Tuple<double, double>) optionalArguments[0];
                var (item3, item4) = (Tuple<double, double>) optionalArguments[1];
                var (item5, item6) = (Tuple<double, double>) optionalArguments[2];

                const double p1 = 0.1;
                const double p2 = 0.9;
                const double p3 = 0.0;
                
                var y1A = item1 * p1;
                var y1B = Math.Sqrt(item1 * item2) * p1;
                var y1C = item2 * p1;
                
                var y2A = item3 * p2;
                var y2B = Math.Sqrt(item3 * item4) * p2;
                var y2C = item4 * p2;
                
                var y3A = item5 * p3;
                var y3B = Math.Sqrt(item5 * item6) * p3;
                var y3C = item6 * p3;
                
                var listVars = variables.Solutions.ToList();

                return listVars.Count != 6
                    ? new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null)
                    :  MutualInfoHelpers3Variables.MutualInformation3Vars(listVars[0], listVars[1], listVars[2], Interval.Zero, Interval.Zero, Interval.Zero,
                        y1A, y1B, y1C, y2A, y2B, y2C, y3A, y3B, y3C, variables.GradientTagged, variables.GradientSolution, variables.Gradient);
            },
            Constraints = variables =>
            {
                var enumerable = variables.Solutions as Interval[] ?? variables.Solutions.ToArray();
                if (enumerable.Length != 6)
                {
                    return false;
                }

                var listVars = enumerable.ToList();
                // Bounds
                var toKeep = true;
                
                var ctr1 = -listVars[0];
                var ctr2 =  listVars[0] - 1;
                var ctr3 = -listVars[1] - 1;
                var ctr4 =  listVars[1] - 1;
                var ctr5 = -listVars[2];
                var ctr6 = listVars[2] - 1;
                var ctr7 = -listVars[3];
                var ctr8 =  listVars[3] - 1;
                var ctr9 = -listVars[4] - 1;
                var ctr10 =  listVars[4] - 1;
                var ctr11 = -listVars[5];
                var ctr12 = listVars[5] - 1;
                // // Positive semi-definite
                var ctr13 = -listVars[0] * listVars[2] + Core.IntervalMath.Sqr(listVars[1]);
                var ctr14 = -listVars[3] * listVars[5] + Core.IntervalMath.Sqr(listVars[4]);
                var ctr15 = -(1 - listVars[0] - listVars[3]) * (1 - listVars[2] - listVars[5]) + Core.IntervalMath.Sqr(-listVars[1]-listVars[4]);
                var ctr16 = (listVars[0] + listVars[2] - 1);
                var ctr17 = (listVars[3] + listVars[5] - 1);
               
                var ctrContains = new List<bool>
                {
                    ctr1.ContainsZero(),
                    ctr2.ContainsZero(),
                    ctr3.ContainsZero(),
                    ctr4.ContainsZero(),
                    ctr5.ContainsZero(),
                    ctr6.ContainsZero(),
                    ctr7.ContainsZero(),
                    ctr8.ContainsZero(),
                    ctr9.ContainsZero(),
                    ctr10.ContainsZero(),
                    ctr11.ContainsZero(),
                    ctr12.ContainsZero(),
                    ctr13.ContainsZero(),
                    ctr14.ContainsZero(),
                    ctr15.ContainsZero(),
                    ctr16.ContainsZero(),
                    ctr17.ContainsZero(),
                };
                
                var ctr1Inside = ctr1.Supremum <= 0;
                var ctr2Inside = ctr2.Supremum <= 0;
                var ctr3Inside = ctr3.Supremum <= 0;
                var ctr4Inside = ctr4.Supremum <= 0;
                var ctr5Inside = ctr5.Supremum <= 0;
                var ctr6Inside = ctr6.Supremum <= 0;
                var ctr7Inside = ctr7.Supremum <= 0;
                var ctr8Inside = ctr8.Supremum <= 0;
                var ctr9Inside = ctr9.Supremum <= 0;
                var ctr10Inside = ctr10.Supremum <= 0;
                var ctr11Inside = ctr11.Supremum <= 0;
                var ctr12Inside = ctr12.Supremum <= 0;
                var ctr13Inside = ctr13.Supremum <= 0;
                var ctr14Inside = ctr14.Supremum <= 0;
                var ctr15Inside = ctr15.Supremum <= 0;
                
                var ctr1Outside = ctr1.Infimum > 0;
                var ctr2Outside = ctr2.Infimum > 0;
                var ctr3Outside = ctr3.Infimum > 0;
                var ctr4Outside = ctr4.Infimum > 0;
                var ctr5Outside = ctr5.Infimum > 0;
                var ctr6Outside = ctr6.Infimum > 0;
                var ctr7Outside = ctr7.Infimum > 0;
                var ctr8Outside = ctr8.Infimum > 0;
                var ctr9Outside = ctr9.Infimum > 0;
                var ctr10Outside = ctr10.Infimum > 0;
                var ctr11Outside = ctr11.Infimum > 0;
                var ctr12Outside = ctr12.Infimum > 0;
                var ctr13Outside = ctr13.Infimum > 0;
                var ctr14Outside = ctr14.Infimum > 0;
                var ctr15Outside = ctr15.Infimum > 0;
                
                if (ctr1Inside && ctr2Inside && ctr3Inside && ctr4Inside && ctr5Inside && ctr6Inside && ctr7Inside && ctr8Inside && ctr9Inside && ctr10Inside && ctr11Inside && ctr12Inside && ctr13Inside && ctr14Inside && ctr15Inside)
                // if (ctr1Inside && ctr2Inside && ctr3Inside && ctr4Inside && ctr5Inside && ctr6Inside)
                    toKeep = false; // red boxes

                // if (ctr1Outside || ctr2Outside || ctr3Outside || ctr4Outside || ctr5Outside || ctr6Outside || ctr7Outside || ctr8Outside)
                if (ctr1Outside || ctr2Outside || ctr3Outside || ctr4Outside || ctr5Outside || ctr6Outside  || ctr7Outside || ctr8Outside || ctr9Outside || ctr10Outside || ctr11Outside || ctr12Outside  || ctr13Outside || ctr14Outside || ctr15Outside)
                    toKeep = false; // Blue boxes

                // 0 in g_j([x]) AND forall(i != j) 0 not in g_i([x]) AND dg_j // df AND 0 not in [df]([x]) => x* not solution
                // lock (_lockingPrintGradient)
                // {
                // Only one constraint verified
                var countConstraints = 0;
                var indexLastConstraint = 0;
                for (var i = 0; i < ctrContains.Count; i++)
                {
                    if (!ctrContains[i]) continue;
                    countConstraints++;
                    indexLastConstraint = i;
                }

                // Are grad(g) and grad(f) independent ?
                var indep = false;
                if (countConstraints == 1 && variables.Gradient != null)
                {
                    switch (indexLastConstraint)
                    {
                        case 0:
                        case 1:
                        {
                            indep = variables.Gradient[1].ContainsZero() 
                                    && variables.Gradient[2].ContainsZero() 
                                    && variables.Gradient[3].ContainsZero() 
                                    && variables.Gradient[4].ContainsZero()
                                    && variables.Gradient[5].ContainsZero();
                            break;
                        }
                        case 2:
                        case 3:
                        {
                            indep = variables.Gradient[0].ContainsZero() 
                                    && variables.Gradient[2].ContainsZero() 
                                    && variables.Gradient[3].ContainsZero() 
                                    && variables.Gradient[4].ContainsZero()
                                    && variables.Gradient[5].ContainsZero();
                            break;
                        }
                        case 4:
                        case 5:
                        {
                            indep = variables.Gradient[0].ContainsZero() 
                                    && variables.Gradient[1].ContainsZero() 
                                    && variables.Gradient[3].ContainsZero() 
                                    && variables.Gradient[4].ContainsZero()
                                    && variables.Gradient[5].ContainsZero();
                            break;
                        }
                        case 6:
                        case 7:
                        {
                            indep = variables.Gradient[0].ContainsZero() 
                                    && variables.Gradient[1].ContainsZero() 
                                    && variables.Gradient[2].ContainsZero() 
                                    && variables.Gradient[4].ContainsZero()
                                    && variables.Gradient[5].ContainsZero();
                            break;
                        }
                        case 8:
                        case 9:
                        {
                            indep = variables.Gradient[0].ContainsZero() 
                                    && variables.Gradient[1].ContainsZero() 
                                    && variables.Gradient[2].ContainsZero() 
                                    && variables.Gradient[3].ContainsZero()
                                    && variables.Gradient[5].ContainsZero();
                            break;
                        }
                        case 10:
                        case 11:
                        {
                            indep = variables.Gradient[0].ContainsZero() 
                                    && variables.Gradient[1].ContainsZero() 
                                    && variables.Gradient[2].ContainsZero() 
                                    && variables.Gradient[3].ContainsZero()
                                    && variables.Gradient[4].ContainsZero();
                            break;
                        }
                    }
                }
                if (indep)
                {
                    // lock (_lockingPrintGradient)
                    // {
                    //     Console.WriteLine("INDEP !!!");
                    // }
                    toKeep = !variables.GradientTagged;
                    // if (!toKeep) toKeep = false;
                }
                // }
                if (!ctrContains[15]) toKeep = false;
                if (!ctrContains[16]) toKeep = false;
                
                // if(!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 1;
                // else if ( ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 2;
                // else if (!ctr1Contains &&  ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 3;  //<== green
                // else if (!ctr1Contains && !ctr2Contains &&  ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 4;
                // else if (!ctr1Contains && !ctr2Contains && !ctr3Contains &&  ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 5;
                // else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains &&  ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 6;
                // else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains &&  ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 7;
                // else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains &&  ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 8;  //<== white eb
                // else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains &&  ctr8Contains) variables.RespectedConstraints = 9;  //<== yellow
                // else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains &&  ctr7Contains &&  ctr8Contains) variables.RespectedConstraints = 10; //<== blue
                // else if ( ctr1Contains &&  ctr2Contains &&  ctr3Contains &&  ctr4Contains &&  ctr5Contains &&  ctr6Contains &&  ctr7Contains &&  ctr8Contains) variables.RespectedConstraints = 11; //<== gray 7f
                // else variables.RespectedConstraints = 12; //<== pink 2

                return toKeep;
            }
        };


        private static ProblemDescriptor ProblemMutualInformation3 => new()
        {
            ProblemFunctionDescription = "Mutual Information with 3 variables",
            ProblemConstraintsDescription = "",
            Function = (variables, optionalArguments) =>
            {
                if (optionalArguments.Count != 3)
                    return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null);
                if(!(optionalArguments[0] is Tuple<double, double>)) return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null);
                if(!(optionalArguments[1] is Tuple<double, double>)) return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null);
                if(!(optionalArguments[2] is Tuple<double, double>)) return new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null);
                
                var (item1, item2) = (Tuple<double, double>) optionalArguments[0];
                var (item3, item4) = (Tuple<double, double>) optionalArguments[1];
                var (item5, item6) = (Tuple<double, double>) optionalArguments[2];

                const double p1 = 0.1;
                const double p2 = 0.6;
                const double p3 = 0.3;
                
                var y1A = item1 * p1;
                var y1B = Math.Sqrt(item1 * item2) * p1;
                var y1C = item2 * p1;
                
                var y2A = item3 * p2;
                var y2B = Math.Sqrt(item3 * item4) * p2;
                var y2C = item4 * p2;
                
                var y3A = item5 * p3;
                var y3B = Math.Sqrt(item5 * item6) * p3;
                var y3C = item6 * p3;

                var listVars = variables.Solutions.ToList();
                return listVars.Count != 6
                    ? new Tuple<Interval, bool, int, IEnumerable<Interval>>(Interval.FromInfSup(double.NaN, double.NaN), false, -1, null)
                    // : Interval.Zero;
                    : MutualInfoHelpers3Variables.MutualInformation3Vars(
                        listVars[0], listVars[1], listVars[2], listVars[3], listVars[4], listVars[5], 
                        y1A, y1B, y1C, y2A, y2B, y2C, y3A, y3B, y3C, variables.GradientTagged, variables.GradientSolution, variables.Gradient);

            },
            Constraints = variables =>
            {
                var enumerable = variables.Solutions as Interval[] ?? variables.Solutions.ToArray();
                if (enumerable.Length != 6)
                {
                    return false;
                }

                var listVars = enumerable.ToList();
                // Bounds
                var toKeep = true;
                
                var ctr1 = -listVars[0];
                var ctr2 =  listVars[0] - 1;
                var ctr3 = -listVars[1] - 1;
                var ctr4 =  listVars[1] - 1;
                var ctr5 = -listVars[2];
                var ctr6 = listVars[2] - 1;
                var ctr7 = -listVars[3];
                var ctr8 =  listVars[3] - 1;
                var ctr9 = -listVars[4] - 1;
                var ctr10 =  listVars[4] - 1;
                var ctr11 = -listVars[5];
                var ctr12 = listVars[5] - 1;
                // // Positive semi-definite
                var ctr13 = -listVars[0] * listVars[2] + Core.IntervalMath.Sqr(listVars[1]);
                var ctr14 = -listVars[3] * listVars[5] + Core.IntervalMath.Sqr(listVars[4]);
                var ctr15 = -(1 - listVars[0] - listVars[3]) * (1 - listVars[2] - listVars[5]) + Core.IntervalMath.Sqr(-listVars[1]-listVars[4]);
                var ctr16 = (listVars[0] + listVars[2] - 1);
                
                
                var ctr1Contains = ctr1.ContainsZero();
                var ctr2Contains = ctr2.ContainsZero();
                var ctr3Contains = ctr3.ContainsZero();
                var ctr4Contains = ctr4.ContainsZero();
                var ctr5Contains = ctr5.ContainsZero();
                var ctr6Contains = ctr6.ContainsZero();
                var ctr7Contains = ctr7.ContainsZero();
                var ctr8Contains = ctr8.ContainsZero();
                var ctr9Contains = ctr9.ContainsZero();
                var ctr10Contains = ctr10.ContainsZero();
                var ctr11Contains = ctr11.ContainsZero();
                var ctr12Contains = ctr12.ContainsZero();
                var ctr13Contains = ctr13.ContainsZero();
                var ctr14Contains = ctr14.ContainsZero();
                var ctr15Contains = ctr15.ContainsZero();
                var ctr16Contains = ctr16.ContainsZero();
                
                var ctr1Inside = ctr1.Supremum <= 0;
                var ctr2Inside = ctr2.Supremum <= 0;
                var ctr3Inside = ctr3.Supremum <= 0;
                var ctr4Inside = ctr4.Supremum <= 0;
                var ctr5Inside = ctr5.Supremum <= 0;
                var ctr6Inside = ctr6.Supremum <= 0;
                var ctr7Inside = ctr7.Supremum <= 0;
                var ctr8Inside = ctr8.Supremum <= 0;
                var ctr9Inside = ctr9.Supremum <= 0;
                var ctr10Inside = ctr10.Supremum <= 0;
                var ctr11Inside = ctr11.Supremum <= 0;
                var ctr12Inside = ctr12.Supremum <= 0;
                var ctr13Inside = ctr13.Supremum <= 0;
                var ctr14Inside = ctr14.Supremum <= 0;
                var ctr15Inside = ctr15.Supremum <= 0;
                
                var ctr1Outside = ctr1.Infimum > 0;
                var ctr2Outside = ctr2.Infimum > 0;
                var ctr3Outside = ctr3.Infimum > 0;
                var ctr4Outside = ctr4.Infimum > 0;
                var ctr5Outside = ctr5.Infimum > 0;
                var ctr6Outside = ctr6.Infimum > 0;
                var ctr7Outside = ctr7.Infimum > 0;
                var ctr8Outside = ctr8.Infimum > 0;
                var ctr9Outside = ctr9.Infimum > 0;
                var ctr10Outside = ctr10.Infimum > 0;
                var ctr11Outside = ctr11.Infimum > 0;
                var ctr12Outside = ctr12.Infimum > 0;
                var ctr13Outside = ctr13.Infimum > 0;
                var ctr14Outside = ctr14.Infimum > 0;
                var ctr15Outside = ctr15.Infimum > 0;
                
                if (ctr1Inside && ctr2Inside && ctr3Inside && ctr4Inside && ctr5Inside && ctr6Inside && ctr7Inside && ctr8Inside && ctr9Inside && ctr10Inside && ctr11Inside && ctr12Inside && ctr13Inside && ctr14Inside && ctr15Inside)
                // if (ctr1Inside && ctr2Inside && ctr3Inside && ctr4Inside && ctr5Inside && ctr6Inside)
                    toKeep = false; // red boxes

                // if (ctr1Outside || ctr2Outside || ctr3Outside || ctr4Outside || ctr5Outside || ctr6Outside || ctr7Outside || ctr8Outside)
                if (ctr1Outside || ctr2Outside || ctr3Outside || ctr4Outside || ctr5Outside || ctr6Outside  || ctr7Outside || ctr8Outside || ctr9Outside || ctr10Outside || ctr11Outside || ctr12Outside  || ctr13Outside || ctr14Outside || ctr15Outside)
                    toKeep = false; // Blue boxes

                if (!ctr16Contains) toKeep = false;
                
                // if(!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 1;
                // else if ( ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 2;
                // else if (!ctr1Contains &&  ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 3;  //<== green
                // else if (!ctr1Contains && !ctr2Contains &&  ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 4;
                // else if (!ctr1Contains && !ctr2Contains && !ctr3Contains &&  ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 5;
                // else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains &&  ctr5Contains && !ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 6;
                // else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains &&  ctr6Contains && !ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 7;
                // else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains &&  ctr7Contains && !ctr8Contains) variables.RespectedConstraints = 8;  //<== white eb
                // else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains && !ctr7Contains &&  ctr8Contains) variables.RespectedConstraints = 9;  //<== yellow
                // else if (!ctr1Contains && !ctr2Contains && !ctr3Contains && !ctr4Contains && !ctr5Contains && !ctr6Contains &&  ctr7Contains &&  ctr8Contains) variables.RespectedConstraints = 10; //<== blue
                // else if ( ctr1Contains &&  ctr2Contains &&  ctr3Contains &&  ctr4Contains &&  ctr5Contains &&  ctr6Contains &&  ctr7Contains &&  ctr8Contains) variables.RespectedConstraints = 11; //<== gray 7f
                // else variables.RespectedConstraints = 12; //<== pink 2
                //
                return toKeep;
            }
        };
        
        public static readonly List<ProblemDescriptor> Problems2D = new()
        {
            ProblemX1X2,
            ProblemX1X2Square,
            ProblemX1X2Ineq,
            ProblemX1X2Ineq2,
            ProblemX1X2Ineq3,
            ProblemX1X2Ineq4,
            ProblemX1SqrX2,
            ProblemXLogX1,
            ProblemXLogX2
        };

        public static readonly List<ProblemDescriptor> Problems3D = new()
        {
            ProblemXLogX3,
            ProblemMutualInformation,
            ProblemMutualInformation3,
            ProblemMI3
        };
    }
}