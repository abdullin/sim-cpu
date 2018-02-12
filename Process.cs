using System.Collections.Generic;

namespace ConsoleApp1 {
    public class Process {
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
    
    
    public enum Resource {
        CPU,
        IO,
    }

    public class Burst {
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
}