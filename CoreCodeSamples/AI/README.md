# AI System

**Focus:**  
Controls enemy unit decision-making and tactical behavior during combat turns.  
Implements a heuristic-based evaluation system to determine optimal actions, enabling AI units to make context-aware strategic decisions in a turn-based environment.

**Key Scripts:**  
- **EnemyAI.cs** – Manages the enemy turn flow and decision-making process; handles AI states such as *WaitingForEnemyTurn*, *TakingTurn*, and *Busy*, and selects the best action based on unit state and tactical context.  
- **EnemyAIAction.cs** – Represents a potential AI action with its target position and evaluated action value; used by `EnemyAI` to compare, rank, and execute optimal tactical choices.
