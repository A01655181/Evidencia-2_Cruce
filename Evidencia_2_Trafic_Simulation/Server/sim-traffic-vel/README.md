# M1 activity
This is the M1 activity to show statistics for an active cleaning robot

### Inputs
The required inputs are:

- Room of MxN.
- Number of agents
- Initial percetange of dirty cells
- Execution in time of T seconds

### Simulation
The steps to follow for the simulation are:

- Start empty dirty cells (random locations)

- All roomba start at [1,1].

- In every step:
	- If cell is dirty then cleans.
	- If cell is clean, agent chooses a random location and moves, if cannot move then stays.

- Program executes in established time

### Analyze the following:
- Max time until all cells were cleaned (or max time)
- Percentage of cleaned cells at the end of sim.
- Number of movements made by all agents
- How number of agents impacst cleaning time and number of movements
