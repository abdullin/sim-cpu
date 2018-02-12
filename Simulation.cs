using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace ConsoleApp1 {
    public sealed class Simulation {
        
        private readonly SortedList<int, List<Command>> _inbox = new SortedList<int, List<Command>>();

        public void Add(int time, Command e) {
            if (Const.Debug)
                Console.WriteLine($"Schedule {e.Type} for P{e.ProcessId} at T+{time}");
            List<Command> list;
            if (!_inbox.TryGetValue(time, out list)) {
                list = new List<Command>();
                _inbox.Add(time, list);
            }

            list.Add(e);
        }
        
        public int Time { get; private set; }
        public bool Debug { get; set; }

        public bool FastForwardToNextCommand(Func<Command,int> priority, out Command cmd) {
            
            if (_inbox.Count == 0) {
                cmd = null;
                return false;
            }

            var l = _inbox.First();


            var list = l.Value;

            if (list.Count > 1) {
                list.Sort((@lf, @r) => priority(lf).CompareTo(priority(r)));
                //Console.WriteLine("Competing futures: {0}", string.Join(", ", list.Select(x => x.Type)));
            }

            var time = l.Key;
            
            if (time > Time) {
                // advance the simulation time
                Time = time;
                if (Debug)
                    Console.WriteLine("Fast forward: T+" + time);
            }
            cmd = list[0];
            list.RemoveAt(0);
            if (list.Count == 0) {
                _inbox.RemoveAt(0);
            }

            
            return true;
        }
    }
}