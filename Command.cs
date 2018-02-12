namespace ConsoleApp1 {
    public class Command {
        public readonly int ProcessId;
        public readonly CommandType Type;
        public readonly Process Process;
        

        public Command(CommandType type, int processId, Process process = null) {
            Type = type;
            ProcessId = processId;
            Process = process;
        }
    }
    
    

    public enum CommandType {
        Arrival,
        Preemption,
        Termination,
        IORequest,
        IOCompletion
    }
}