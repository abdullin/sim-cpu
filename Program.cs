using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace ConsoleApp1 {
    public static class Const {
        public static bool Debug = true;
    }

    internal class Program {
        static Process Parse(int pid, string source) {
            var items = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var arrival = int.Parse(items[0]);

            var list = new Queue<Burst>();


            for (int i = 2; i < items.Length; i++) {
                var type = (i % 2 == 0) ? Resource.CPU : Resource.IO;
                var duration = int.Parse(items[i]);
                list.Enqueue(new Burst(type, duration));
            }

            //list.Enqueue(new Burst(Resource.KILL, 0));
            return new Process(pid, list, arrival);
        }


        private static void Main(string[] args) {
            const int quantum = 2;

            const string input =
                @"3 3 2 5 8 7 4
4 1 4
6 3 2 5 2 7 4
8 4 8 2 10 2 7 5 6
10 2 1 10 2
13 4 1 15 1 12 4 8 6";

            var procs = input.Trim()
                .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Select((s, i) => Parse(i, s))
                .ToList();


            var future = new Inbox();
            var readyQueue = new Queue<Process>();
            var ioQueue = new Queue<Process>();

            for (int i = 0; i < procs.Count; i++) {
                var proc = procs[i];
                future.Add(proc.Arrival, new Event(EventType.Arrival, i));
            }

            var time = 0;
            var cpuIdle = true;
            var ioIdle = true;


            KeyValuePair<int, Event> e;
            while (future.TryGetNext(out e)) {
                var t = e.Key;

                if (time < t) {
                    // advance the simulation time
                    time = t;
                    if (Const.Debug)
                        Console.WriteLine("Fast forward: T+" + time);
                }

                var pid = e.Value.ProcessId;


                var proc = procs[pid];
                switch (e.Value.Type) {
                    case EventType.Arrival:
                        readyQueue.Enqueue(proc);
                        proc.ReadyAdded = time;
                        break;
                    case EventType.Preemption:
                        cpuIdle = true;
                        readyQueue.Enqueue(proc);
                        proc.ReadyAdded = time;
                        break;
                    case EventType.Termination:
                        proc.Terminated = time;
                        
                        cpuIdle = true;
                        break;
                    case EventType.IORequest:
                        ioQueue.Enqueue(proc);
                        proc.IOAdded = time;
                        cpuIdle = true;
                        break;
                    case EventType.IOCompletion:
                        ioIdle = true;
                        readyQueue.Enqueue(proc);
                        proc.ReadyAdded = time;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("event type", e.Value.Type.ToString(), "out of range");
                }

                Process cpuProc;
                if (cpuIdle && readyQueue.TryDequeue(out cpuProc)) {
                    cpuIdle = false;
                    cpuProc.ReadyWait += time - cpuProc.ReadyAdded;
                    DispatchCPUProcess(cpuProc, quantum, future, time);
                }

                Process ioProc;
                if (ioIdle && ioQueue.TryDequeue(out ioProc)) {
                    ioIdle = false;
                    ioProc.IOWait += time - ioProc.IOAdded;
                    DispatchIOOperation(ioProc, quantum, future, time);
                }
            }

            Console.WriteLine("Simulation complete! Statistics:");

            foreach (var proc in procs) {
                var tat = proc.Terminated - proc.Arrival;
                        
                Console.WriteLine(
                    $"P{proc.ID} (TAT = {tat}, ReadyWait = {proc.ReadyWait}, I/O-wait={proc.IOWait})");
            }
        }

        static void DispatchIOOperation(Process result, int quantum, Inbox future, int time) {
            var burst = result.Bursts.Dequeue();
            if (burst.Resource != Resource.IO) {
                throw new InvalidOperationException("Must be an IO operation");
            }
            
            

            if (Const.Debug) {
                Console.WriteLine($"Start IO for {burst.Duration}");
            }
            burst.ReduceBy(burst.Duration);

            future.Add(time + burst.Duration, new Event(EventType.IOCompletion, result.ID));
        }

        private static void DispatchCPUProcess(Process result, int quantum, Inbox future, int time) {
            var burst = result.Bursts.Peek();


            if (burst.Remain() > quantum) {
                if (Const.Debug) {
                    Console.WriteLine($"Start CPU for {quantum}");
                }
                burst.ReduceBy(quantum);
                future.Add(time + quantum, new Event(EventType.Preemption, result.ID));
                return;
            }

            // execute the remaining CPU bit
            var duration = burst.Remain();
            result.Bursts.Dequeue();
            
            if (Const.Debug) {
                Console.WriteLine($"Start CPU for {duration}");
            }

            burst.ReduceBy(duration);


            if (result.Bursts.Count == 0) {
                future.Add(time + duration, new Event(EventType.Termination, result.ID));
                return;
            }

            // schedule the next

            switch (result.Bursts.Peek().Resource) {
                case Resource.CPU:
                    throw new InvalidOperationException();
                case Resource.IO:
                    future.Add(time + duration, new Event(EventType.IORequest, result.ID));
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private class Process {
            public readonly Queue<Burst> Bursts;
            public readonly int Arrival;
            public int Terminated;
            public readonly int ID;

            public int IOAdded;
            public int ReadyAdded;
            public int IOWait;
            public int ReadyWait;

            public Process(int pid, Queue<Burst> bursts, int arrival) {
                ID = pid;
                Bursts = bursts;
                Arrival = arrival;
            }
        }


        class Burst {
            public readonly Resource Resource;
            public readonly int Duration;

            private int _executed;

            public void ReduceBy(int duration) {
                _executed += duration;
            }

            public int Remain() {
                return Duration - _executed;
            }


            public Burst(Resource resource, int duration) {
                Resource = resource;
                Duration = duration;
            }
        }

        enum Resource {
            CPU,
            IO,
            
        }
    }

    public class Event {
        public readonly EventType Type;
        public readonly int ProcessId;

        public Event(EventType type, int processId) {
            Type = type;
            ProcessId = processId;
        }
    }

    public enum EventType {
        Arrival,
        Preemption,
        Termination,
        IORequest,
        IOCompletion,
    }
}