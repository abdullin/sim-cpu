# SimCPU

This is an event-driven simulation of a process scheduling algorithm
on a machine with one CPI and IO device.

Implementation is based on a random project assignment I found on the
Internet:
[CS452 Project Job Scheduling Simulation](http://www.cis.gvsu.edu/~dulimarh/CS452/Projects/JS/).




The implementation almost works. For some reason, while running the
mixed job workload with time quantum=2:

```
3 3 2 5 8 7 4
4 1 4
6 3 2 5 2 7 4
8 4 8 2 10 2 7 5 6
10 2 1 10 2
13 4 1 15 1 12 4 8 6
```

the expected result is:
```
P0 (TAT = 60, ReadyWait = 18, I/O-wait=16)
P1 (TAT = 7, ReadyWait = 3, I/O-wait=0)
P2 (TAT = 47, ReadyWait = 7, I/O-wait=20)
P3 (TAT = 86, ReadyWait = 24, I/O-wait=22)
P4 (TAT = 19, ReadyWait = 5, I/O-wait=1)
P5 (TAT = 75, ReadyWait = 7, I/O-wait=21)
```

while my simulation returns:

```
P0 (TAT = 60, ReadyWait = 18, I/O-wait=16)
P1 (TAT = 7, ReadyWait = 3, I/O-wait=0)
P2 (TAT = 47, ReadyWait = 7, I/O-wait=20)
P3 (TAT = 86, ReadyWait = 24, I/O-wait=22)
P4 (TAT = 19, ReadyWait = 5, I/O-wait=1)
P5 (TAT = 75, ReadyWait = 5, I/O-wait=23)
```


Note the different `ReadyWait` and `I/O-wait` values for `P5`.
