# IntervalEval
This project is the result of the work done during the 5 month internship at LARIS (University of Angers). The goal was to build an optimal quantum detector using the mutual information criterion, combined with interval analysis methods to solve the problem. 

This code was moved from its [original repository](https://github.com/PierreEngelstein/MasterRecherche) containing all the work done during the full master year.

## Building
This project uses [dotnet 5](https://dotnet.microsoft.com/download/dotnet/5.0) and will need to be upgraded to [dotnet 6](https://dotnet.microsoft.com/download/dotnet/6.0) when the final release is out.

To get dotnet 5 / 6, follow the instructions provided by Microsoft depending on your operating system:

* On linux: https://docs.microsoft.com/en-us/dotnet/core/install/linux
* On Windows: https://docs.microsoft.com/en-us/dotnet/core/install/windows?tabs=net50
* On MacOS: https://docs.microsoft.com/en-us/dotnet/core/install/macos

If you are on linux, the script `build_all.sh` provides an easy way to directly build all the executables for all targets (windows and linux) using the release configuration and single-file standalone option. You still need to download and setup dotnet to use this script.

### Web Interface
To build and run the web interface version, execute the following commands from the root firectory of the project:

```
dotnet clean
dotnet restore
cd IntervalEval.Front
dotnet run
```

This project uses [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor), specifically the `Server` hosting model.

### Console Interface
To build and run the console interface version, execute the following commands from the root firectory of the project:

```
dotnet clean
dotnet restore
cd IntervalEval.Optimizer
dotnet run
```

For this project, you can enter specific configuration in the [`configuration.json`](IntervalEval.Optimizer/configuration.json) file (input squred quantum states, ranges of evaluation and amount of iterations).

### Other projects
The [IntervalEval.Tests](IntervalEval.Tests/) project contains some unit tests.

The [IntervalEval.FrontConsole](IntervalEval.FrontConsole/) project contains some code tests, in particular tests for 3 states as input of the problem to check optimization time and memory usage for larger problems.

The [IntervalEval](IntervalEval/) project is the interval library, based on the [IntSharp](https://github.com/selmaohneh/IntSharp) library with some modifications added to better suit our need for the optimizer.

The [IbexCodes](IbexCodes/) folder contains equivalent codes for solving the problem using [ibex](https://www.ibex-lib.org/) (ibexopt). This is used for comparison of performance of our code compared to the built-in optimizer of ibex.