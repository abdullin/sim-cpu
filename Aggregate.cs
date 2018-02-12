using System;
using System.Collections.Generic;

namespace ConsoleApp1 {
    public class Aggregate {
        readonly Queue<Process> _ioQueue = new Queue<Process>();
        readonly Queue<Process> _readyQueue = new Queue<Process>();
        readonly List<Process> _processes = new List<Process>();

        readonly Simulation _sim;
        readonly int _timeQuantum;
        
        bool _cpuIdle = true;
        bool _ioIdle = true;

        public Aggregate(int timeQuantum, Simulation sim) {
            _timeQuantum = timeQuantum;
            _sim = sim;
        }

        public void HandleCommand(Command cmd) {
            if (cmd.Type == CommandType.Arrival) {
                _processes.Add(cmd.Process);
            }
            
            var pid = cmd.ProcessId;

            var proc = _processes[pid];
            switch (cmd.Type) {
                case CommandType.Arrival:
                    _readyQueue.Enqueue(proc);
                    proc.ReadyAdded = _sim.Time;
                    break;
                case CommandType.Preemption:
                    _cpuIdle = true;
                    _readyQueue.Enqueue(proc);
                    proc.ReadyAdded = _sim.Time;
                    break;
                case CommandType.Termination:
                    proc.Terminated = _sim.Time;
                    _cpuIdle = true;
                    _sim.Debug($"P{pid} terminated");
                    break;
                case CommandType.IORequest:
                    _ioQueue.Enqueue(proc);
                    proc.IOAdded = _sim.Time;
                    _cpuIdle = true;
                    break;
                case CommandType.IOCompletion:
                    _ioIdle = true;
                    _readyQueue.Enqueue(proc);
                    proc.ReadyAdded = _sim.Time;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("command type", cmd.Type.ToString(), "out of range");
            }

            if (_cpuIdle && _readyQueue.TryDequeue(out var cpuProc)) {
                _cpuIdle = false;
                cpuProc.ReadyWait += _sim.Time - cpuProc.ReadyAdded;
                DispatchCPUProcess(cpuProc);
            }

            if (_ioIdle && _ioQueue.TryDequeue(out var ioProc)) {
                _ioIdle = false;
                ioProc.IOWait += _sim.Time - ioProc.IOAdded;
                DispatchIOOperation(ioProc);
            }
        }

        void DispatchIOOperation(Process result) {
            var burst = result.Bursts.Dequeue();
            if (burst.Resource != Resource.IO) 
                throw new InvalidOperationException("Must be an IO operation");

            _sim.Debug($"P{result.ID} started IO for T{burst.Duration}");

            burst.ReduceAmountBy(burst.Duration);

            _sim.Schedule(burst.Duration, new Command(CommandType.IOCompletion, result.ID));
        }


        public int CommandPriority(Command e) {
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
                var tat = proc.Terminated - proc.ArrivalTime;

                Console.WriteLine(
                    $"P{proc.ID} (TAT = {tat}, ReadyWait = {proc.ReadyWait}, I/O-wait={proc.IOWait})");
            }
        }

        void DispatchCPUProcess(Process result) {
            var burst = result.Bursts.Peek();


            if (burst.Remain() > _timeQuantum) {
                _sim.Debug($"P{result.ID} started CPU for T{_timeQuantum}");

                burst.ReduceAmountBy(_timeQuantum);
                _sim.Schedule(_timeQuantum, new Command(CommandType.Preemption, result.ID));
                return;
            }

            // execute the remaining CPU bit
            var duration = burst.Remain();
            result.Bursts.Dequeue();


            _sim.Debug($"P{result.ID} started CPU for T{duration}");

            burst.ReduceAmountBy(duration);


            if (result.Bursts.Count == 0) {
                _sim.Schedule(duration, new Command(CommandType.Termination, result.ID));
                return;
            }

            switch (result.Bursts.Peek().Resource) {
                case Resource.CPU:
                    throw new InvalidOperationException("CPU burst can't follow CPU burst");
                case Resource.IO:
                    _sim.Schedule(duration, new Command(CommandType.IORequest, result.ID));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}