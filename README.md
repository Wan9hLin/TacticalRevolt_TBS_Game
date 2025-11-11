# Tactical Revolt

## Overview
**Tactical Revolt** is a turn-based strategy game developed in **Unity**, inspired by **XCOM 2** and **Into the Breach**. The project demonstrates a complete, modular gameplay architecture featuring grid-based combat, pathfinding, unit control, and tactical AI. This repository contains the full Unity project for technical review, along with documentation highlighting the most critical systems and scripts.

---

## Technical Highlights
- **Grid-Based Combat Framework** – Custom coordinate and grid management supporting movement, line of sight, and cover detection.  
- **Modular Action System** – Extensible architecture for all unit actions (`MoveAction`, `ShootAction`, etc.) derived from a shared `BaseAction`.  
- **A* Pathfinding** – Efficient and dynamic pathfinding with live updates to grid walkability when environment changes.  
- **Turn Management** – Centralized `TurnSystem` coordinates player/enemy turns and ensures clean state transitions.  
- **Enemy AI** – Evaluates available actions each turn using a scoring heuristic (`EnemyAIAction`), selecting the most efficient tactical response.  
- **Event-Driven Gameplay** – Uses Unity events for decoupled updates (unit selection, grid refresh, action execution).  

---

## How to Use
1. **Requirements**
   - Unity **2021.3 LTS** or later  
   - Windows 
   - No additional packages required 

2. **Setup**
   - Clone or download the repository.  
   - Open the project in Unity Hub.  
   - Open the main scene located under `Assets/Scenes/StartPage.unity`.  
   - Press **Play** to start.

---

## Notable Systems & Key Scripts
Below are the core gameplay modules and entry points within the project:

### 🟩 Grid System
- `GridSystem.cs` – Core grid management, coordinate conversion, and tile occupancy tracking.  
- `LevelGrid.cs` – Central controller managing unit placement and cover detection.  
- `GridSystemVisual.cs` – Real-time visualization for movement range, target tiles, and cover zones.

### 🎯 Unit & Action System
- `Unit.cs` – Handles health, stats, movement points, and status effects.  
- `UnitActionSystem.cs` – Centralized action execution and player command control.  
- `BaseAction.cs` – Abstract base for all actions with validation and execution flow.  
- `MoveAction.cs` / `ShootAction.cs` – Implement movement and ranged combat behavior.

### 🤖 Enemy AI System
- `EnemyAI.cs` – Controls decision-making and state transitions for AI-controlled units.  
- `EnemyAIAction.cs` – Represents evaluated actions with tactical scoring metrics.

### 🧭 Pathfinding
- `PathFinding.cs` – Implements A* pathfinding with movement cost heuristics and walkability checks.  
- `PathNode.cs` – Stores node data (cost, neighbors, walkable state).  
- `PathfindingUpdater.cs` – Updates the grid when destructible objects affect walkability.

---

## Challenges & Solutions
- **Issue:** Pathfinding grid failed to refresh after object destruction.  
  **Solution:** Added `PathfindingUpdater` to dynamically revalidate affected nodes.

- **Issue:** Enemy AI behavior appeared repetitive and inefficient.  
  **Solution:** Implemented a weighted heuristic scoring system to rank potential actions each turn.

- **Issue:** Overlapping player inputs occasionally triggered invalid action sequences.  
  **Solution:** Centralized validation and action queuing within `UnitActionSystem`.

- **Issue:** Cover detection accuracy was inconsistent on diagonals.  
  **Solution:** Switched from raycast-only checks to directional sampling with layer filtering.

- **Issue:** Visual feedback lagged behind unit updates.  
  **Solution:** Refactored grid updates to use Unity events, ensuring synchronized refreshes.

---

## Project Structure

