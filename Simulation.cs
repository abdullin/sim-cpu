using System;
using System.Collections.Generic;
using System.Linq;

namespace SimCPU {
    public sealed class Simulation {
        readonly SortedList<int, List<Command>> _inbox = new SortedList<int, List<Command>>();

        public int Time { get; private set; }
        public bool Verbose { get; set; }


        public void Debug(string arg) {
            if (Verbose) Console.WriteLine($"T{Time:000}: {arg}");
        }

        public void Schedule(int offset, Command e) {
            var time = offset + Time;
            Debug($"Schedule {e.Type} for P{e.ProcessId} at T{time}");

            if (!_inbox.TryGetValue(time, out var list)) {
                list = new List<Command>();
                _inbox.Add(time, list);
            }

            list.Add(e);
        }

        public bool FastForward(out IList<Command> list) {
            if (_inbox.Count == 0) {
                list = null;
                return false;
            }

            var future = _inbox.First();
            
            list = future.Value;
            var time = future.Key;

            if (time > Time) {
                Time = time;
                // advance the simulation time
                Debug($">>> Fast forward to T{Time} >>>");
            }

            _inbox.RemoveAt(0);
            return true;
        }
    }
}