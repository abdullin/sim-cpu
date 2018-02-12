namespace SimCPU {
    static class Program {
        static void Main(string[] args) {
            const int timeQuantum = 2;
            const string input = @"
3 3 2 5 8 7 4
4 1 4
6 3 2 5 2 7 4
8 4 8 2 10 2 7 5 6
10 2 1 10 2
13 4 1 15 1 12 4 8 6";

            var procs = Parser.ParseString(input);

            var sim = new Simulation {
                Verbose = true
            };

            for (var i = 0; i < procs.Count; i++) {
                var proc = procs[i];
                var cmd = new Command(CommandType.Arrival, i, proc);
                sim.Schedule(proc.ArrivalTime, cmd);
            }

            var agg = new Aggregate(timeQuantum, sim);

            while (sim.FastForward(agg.CommandPriority, out var cmd)) {
                agg.HandleCommand(cmd);
            }

            sim.Debug("No future left, simulation is complete!");
            agg.PrintStatistics();
        }
    }
}