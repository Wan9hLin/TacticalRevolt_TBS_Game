# Action System

**Focus:**  
Defines the modular action framework that governs all unit abilities and interactions within the tactical grid system.  
Provides a consistent structure for extending gameplay actions, enabling both player and AI units to perform context-sensitive moves, attacks, and abilities.

**Key Scripts:**  
- **BaseAction.cs** – Establishes the shared interface and logic for all actions, including validation, start, and completion states; serves as the foundation for extendable tactical behaviors.  
- **MoveAction.cs** – Handles unit movement and pathfinding; integrates with the `PathFinding` system to calculate optimal routes and applies action point costs.  
- **ShootAction.cs** – Manages ranged attack logic, including aiming, hit probability, and damage application; factors in distance, cover, and visual feedback.  
- **InteractAction.cs** – Enables interaction with world objects (doors, switches, devices); validates interaction range and provides AI evaluation values for tactical use.  
- **MindControlAction.cs** – Implements a unique ability allowing temporary control of enemy units; features a multi-phase state machine (aiming, casting, cooldown) with animation synchronization and AI priority scoring.
