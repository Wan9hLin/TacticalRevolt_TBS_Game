# Pathfinding System

**Focus:**  
Implements the A* pathfinding framework that enables tactical unit navigation across the battlefield grid.  
Provides efficient route calculation, walkability updates, and dynamic obstacle handling to ensure smooth and responsive movement for both player and AI units.

**Key Scripts:**  
- **PathFinding.cs** – Core A* pathfinding implementation; calculates optimal routes between grid positions while accounting for terrain cost, cover, and obstacles. Supports dynamic updates to maintain accuracy in destructible environments.  
- **PathNode.cs** – Represents a single node within the grid; stores traversal cost, walkability state, and reference to the previous node to reconstruct movement paths.  
- **PathfindingUpdater.cs** – Monitors and updates grid walkability when environment changes occur (e.g., destructible objects removed), ensuring real-time synchronization with the pathfinding system.
