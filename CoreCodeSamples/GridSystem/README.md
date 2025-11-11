# Grid System

**Focus:**  
Implements the tactical grid framework that defines the battlefield’s spatial logic and interaction layer.  
Manages grid generation, coordinate conversion, cover detection, and real-time visualization to support both gameplay logic and debugging.

**Key Scripts:**  
- **GridSystem.cs** – Core grid management and coordinate mapping; provides world-to-grid conversion utilities, validation, and custom grid initialization for gameplay and debug use.  
- **LevelGrid.cs** – Central controller linking units and grid cells; detects cover using raycasting (distinguishing full and half cover) and triggers gameplay events to update grid state.  
- **GridSystemVisual.cs** – Visual feedback system for the grid; dynamically updates tile colors to reflect move ranges, actions, and cover zones in real time.
