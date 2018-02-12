using System.Collections.Generic;

namespace ConsoleApp1 {
    public class Process {
        public readonly int Arrival;
        public readonly Queue<Burst> Bursts;
        public readonly int ID;

        public int IOAdded;
        public int IOWait;
        public int ReadyAdded;
        public int ReadyWait;
        public int Terminated;

        public Process(int pid, Queue<Burst> bursts, int arrival) {
            ID = pid;
            Bursts = bursts;
            Arrival = arrival;
        }
    }


    public enum Resource {
        CPU,
        IO
    }

    public class Burst {
        public readonly int Duration;
        public readonly Resource Resource;

        int _executed;


        public Burst(Resource resource, int duration) {
            Resource = resource;
            Duration = duration;
        }

        public void ReduceBy(int duration) {
            _executed += duration;
        }

        public int Remain() {
            return Duration - _executed;
        }
    }
}