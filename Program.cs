using System;

namespace ConsoleApp1 {
    class Program {
        static void Main(string[] args) {
            const int quantum = 2;

            const string input =
                @"3 3 2 5 8 7 4
4 1 4
6 3 2 5 2 7 4
8 4 8 2 10 2 7 5 6
10 2 1 10 2
13 4 1 15 1 12 4 8 6";

            var procs = Parser.ParseString(input);
            var simulation = new Simulation {
                Verbose = true
            };


            for (var i = 0; i < procs.Count; i++) {
                var proc = procs[i];
                simulation.Add(proc.Arrival, new Command(CommandType.Arrival, i));
            }

            var agg = new Aggregate(quantum, procs, simulation);


            while (simulation.FastForwardToNextCommand(agg.Priority, out var cmd)) agg.HandleCommand(cmd);
            Console.WriteLine("Simulation complete!");
            agg.PrintStatistics();
        }
    }
}