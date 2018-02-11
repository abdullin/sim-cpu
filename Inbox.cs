using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace ConsoleApp1 {
    public sealed class Inbox {
        private SortedList<int, List<Event>> _inbox = new SortedList<int, List<Event>>();

        public void Add(int time, Event e) {
            if (Const.Debug)
                Console.WriteLine($"Schedule {e.Type} for P{e.ProcessId} at T+{time}");
            List<Event> list;
            if (!_inbox.TryGetValue(time, out list)) {
                list = new List<Event>();
                _inbox.Add(time, list);
            }

            list.Add(e);
            
            
        }


        static int Priority(Event e) {
            switch (e.Type) {
                case EventType.Arrival:
                    return 1;
                case EventType.Preemption:
                    return 3;
                case EventType.Termination:
                    break;
                case EventType.IORequest:
                    break;
                case EventType.IOCompletion:
                    return 2;
                default:
                    return 0;
            }

            return 0;
        }

        public bool TryGetNext(out KeyValuePair<int, Event> e) {
            e = default(KeyValuePair<int, Event>);
            if (_inbox.Count == 0) {
                return false;
            }
            
            

            var l = _inbox.First();


            var list = l.Value;

            if (list.Count > 1) {
                list.Sort((@lf, @r) => Priority(lf).CompareTo(Priority(r)));
                Console.WriteLine("Competing futures: {0}", string.Join(", ", list.Select(x => x.Type)));
                
            }

           

            e = KeyValuePair.Create<int, Event>(l.Key, list[0]);
            list.RemoveAt(0);
            if (list.Count == 0) {
                _inbox.RemoveAt(0);
            }

            return true;
        }
    }
}