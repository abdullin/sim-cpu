using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1 {
    public static class Parser {
        public static Process Parse(int pid, string source) {
            var items = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var arrival = int.Parse(items[0]);

            var list = new Queue<Burst>();


            for (var i = 2; i < items.Length; i++) {
                var type = i % 2 == 0 ? Resource.CPU : Resource.IO;
                var duration = int.Parse(items[i]);
                list.Enqueue(new Burst(type, duration));
            }

            return new Process(pid, list, arrival);
        }

        public static List<Process> ParseString(string input) {
            return input.Trim()
                .Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Select((s, i) => Parse(i, s))
                .ToList();
        }
    }
}