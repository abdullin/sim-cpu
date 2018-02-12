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
            List<Command> list;

            if (!_inbox.TryGetValue(time, out list)) {
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

            var l = _inbox.First();


            list = l.Value;

            var time = l.Key;

            if (time > Time) {
                // advance the simulation time
                Debug($">>> Fast forward to T{time} >>>");
                Time = time;
            }

            _inbox.RemoveAt(0);
            return true;
        }
    }
}