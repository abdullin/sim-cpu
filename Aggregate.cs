using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleApp1 {
    public class Aggregate {
        public readonly int TimeQuantum;
        private readonly List<Process> _processes;
        private readonly Simulation _sim;


        Queue<Process> readyQueue = new Queue<Process>();
        Queue<Process> ioQueue = new Queue<Process>();
        private bool cpuIdle = true;
        private bool ioIdle = true;


        public Aggregate(int timeQuantum, List<Process> processes, Simulation sim) {
            TimeQuantum = timeQuantum;
            _processes = processes;
            _sim = sim;
        }


        public void HandleCommand(Command cmd) {
            var pid = cmd.ProcessId;

            var proc = _processes[pid];
            switch (cmd.Type) {
                case CommandType.Arrival:
                    readyQueue.Enqueue(proc);
                    proc.ReadyAdded = _sim.Time;
                    break;
                case CommandType.Preemption:
                    cpuIdle = true;
                    readyQueue.Enqueue(proc);
                    proc.ReadyAdded = _sim.Time;
                    break;
                case CommandType.Termination:
                    proc.Terminated = _sim.Time;

                    cpuIdle = true;
                    break;
                case CommandType.IORequest:
                    ioQueue.Enqueue(proc);
                    proc.IOAdded = _sim.Time;
                    cpuIdle = true;
                    break;
                case CommandType.IOCompletion:
                    ioIdle = true;
                    readyQueue.Enqueue(proc);
                    proc.ReadyAdded = _sim.Time;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("command type", cmd.Type.ToString(), "out of range");
            }

            Process cpuProc;
            if (cpuIdle && readyQueue.TryDequeue(out cpuProc)) {
                cpuIdle = false;
                cpuProc.ReadyWait += _sim.Time - cpuProc.ReadyAdded;
                DispatchCPUProcess(cpuProc);
            }

            Process ioProc;
            if (ioIdle && ioQueue.TryDequeue(out ioProc)) {
                ioIdle = false;
                ioProc.IOWait += _sim.Time - ioProc.IOAdded;
                DispatchIOOperation(ioProc);
            }
        }

        void DispatchIOOperation(Process result) {
            var burst = result.Bursts.Dequeue();
            if (burst.Resource != Resource.IO) {
                throw new InvalidOperationException("Must be an IO operation");
            }


            if (Const.Debug) {
                Console.WriteLine($"P{result.ID} started IO for T{burst.Duration}");
            }

            burst.ReduceBy(burst.Duration);

            _sim.Add(_sim.Time + burst.Duration, new Command(CommandType.IOCompletion, result.ID));
        }
        
        
        public int Priority(Command e) {
            switch (e.Type) {
                case CommandType.Arrival:
                    return 1;
                case CommandType.Preemption:
                    return 3;
                case CommandType.Termination:
                    break;
                case CommandType.IORequest:
                    break;
                case CommandType.IOCompletion:
                    return 2;
                default:
                    return 0;
            }

            return 0;
        }

        public void PrintStatistics() {
            foreach (var proc in _processes) {
                var tat = proc.Terminated - proc.Arrival;
                        
                Console.WriteLine(
                    $"P{proc.ID} (TAT = {tat}, ReadyWait = {proc.ReadyWait}, I/O-wait={proc.IOWait})");
            }
        }

        void DispatchCPUProcess(Process result) {
            var burst = result.Bursts.Peek();


            if (burst.Remain() > TimeQuantum) {
                if (Const.Debug) {
                    Console.WriteLine($"P{result.ID} started CPU for T{TimeQuantum}");
                }

                burst.ReduceBy(TimeQuantum);
                _sim.Add(_sim.Time + TimeQuantum, new Command(CommandType.Preemption, result.ID));
                return;
            }

            // execute the remaining CPU bit
            var duration = burst.Remain();
            result.Bursts.Dequeue();

            if (Const.Debug) {
                Console.WriteLine($"P{result.ID} started CPU for T{duration}");
            }

            burst.ReduceBy(duration);


            if (result.Bursts.Count == 0) {
                _sim.Add(_sim.Time + duration, new Command(CommandType.Termination, result.ID));
                return;
            }

            switch (result.Bursts.Peek().Resource) {
                case Resource.CPU:
                    throw new InvalidOperationException("CPU burst can't follow CPU burst");
                case Resource.IO:
                    _sim.Add(_sim.Time + duration, new Command(CommandType.IORequest, result.ID));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}