# Multiplayer Framework Comparison
**Graduation Thesis**

## Overview

### Tools and Technologies:
  ### Development Tools
  - **Unity**: Version 2022.3.14f1 LTS (Client)
  - **.NET C# Console**: (Server)

  ### Libraries
  - **WebSocket-Sharp**
  - **Photon Engine**
  - **Netcode for GameObjects**

  ### Monitoring Tools
  - **Multiplayer Network Profiler**
  - **JetBrains dotTrace**
  
### Topic:
We are currently in the golden age of multiplayer games, as evidenced by their significant market share and the millions of players engaging in online gaming daily. However, implementing multiplayer functionality is no small feat, as it presents challenges depending on the type of game and the chosen network architecture. There are various approaches to creating multiplayer games, particularly in the implementation of multiplayer servers.

A robust server is crucial for synchronizing game objects, player positions, stats, matchmaking, and more. Among the many libraries available for creating multiplayer games, three have emerged as dominant choices. Since game requirements vary, this thesis aims to identify the advantages and disadvantages of these three dominant frameworks by evaluating them based on specific criteria.

### Objective
The goal of this thesis is to compare three different approaches to multiplayer game implementation using key criteria such as:
- Lines of code required to implement essential functionalities
- Learning curve associated with each approach
- Performance in standard multiplayer features
- Network capacity in handling game objects
- And more...

To accomplish this, the thesis will develop three identical prototypes of a MUD (Multi-User Dungeon) game, each using a different approach:
1. Custom client-server implementation
2. Photon Engine
3. Netcode for GameObjects

For monitoring and evaluation, the thesis will utilize built-in statistics and the Multiplayer Network Profiler tool to assess the specified criteria.
