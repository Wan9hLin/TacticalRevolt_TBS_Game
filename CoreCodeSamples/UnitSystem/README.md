# Unit System

**Focus:**  
Defines the fundamental logic and data structures for all battlefield units.  
Handles unit attributes, action points, and state transitions, ensuring that movement, combat, and interaction behaviors remain consistent and synchronized across the game systems.

**Key Scripts:**  
- **Unit.cs** – Core component for all units; manages health, action points, and position tracking. Handles key gameplay events such as movement, damage, healing, and death.  
- **UnitActionSystem.cs** – Central controller for unit selection and action execution; validates available actions based on unit state and action points, coordinating player input and game state updates.
