namespace ConsoleApp1 {
    public class Command {
        public readonly int ProcessId;
        public readonly CommandType Type;
        

        public Command(CommandType type, int processId) {
            Type = type;
            ProcessId = processId;
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