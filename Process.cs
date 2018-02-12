using System.Collections.Generic;

namespace SimCPU {
    public class Process {
        public readonly int ArrivalTime;
        public readonly Queue<Burst> Bursts;
        public readonly int ID;

        public int IOAdded;
        public int IOWait;
        public int ReadyAdded;
        public int ReadyWait;
        public int TerminatedTime;

        public Process(int pid, Queue<Burst> bursts, int arrivalTime) {
            ID = pid;
            Bursts = bursts;
            ArrivalTime = arrivalTime;
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

        public void ReduceAmountBy(int duration) {
            _executed += duration;
        }

        public int Remain() {
            return Duration - _executed;
        }
    }
}