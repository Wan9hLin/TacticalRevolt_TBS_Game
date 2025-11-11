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

## Core System Architecture
The project includes a complete Unity implementation, while the **CoreCodeSamples/** folder highlights the most essential gameplay systems for review and documentation.  
Each submodule represents a major component of the game’s technical framework, reflecting modular and scalable system design.


- **CodeSamples/GridSystem/** – Implements the grid-based battlefield and coordinate management utilities.  
  Handles grid creation, world-to-grid conversion, and cover detection logic used by both player and AI units.

- **CodeSamples/UnitSystem/** – Defines the data and logic of all controllable units.  
  Manages unit stats, health, and state transitions, supporting modular action execution and event-driven updates.

- **CodeSamples/ActionSystem/** – Contains the modular action architecture.  
  Provides a base framework for actions (`MoveAction`, `ShootAction`, etc.), handling validation, execution, and action points.

- **CodeSamples/Pathfinding/** – Implements the A* pathfinding algorithm for movement and target evaluation.  
  Includes node data structures, cost-based traversal, and real-time walkability updates.

- **CodeSamples/AI/** – Manages enemy behavior and tactical decision-making.  
  Evaluates possible actions using heuristic scoring to simulate intelligent turn-based strategies.

---

## Links
- 🌐 [Portfolio Page](https://www.henrywang.online/copy-of-chippy-noppo-vr) – Full project breakdown and gameplay demo video 
