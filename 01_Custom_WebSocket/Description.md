# Custom Client-Server Implementation with WebSocketSharp

## Description
This implementation involves the development of a custom client-server architecture using **WebSocketSharp** for WebSocket communication. The project is structured with a Unity-based client and a server built using a Visual Studio console application. This setup allows us to establish real-time, bidirectional communication between the client and server for the Multi-User Dungeon (MUD) game. By manually managing the networking logic, we gain full control over data flow, synchronization of game states, and player interactions, offering a highly customizable multiplayer experience.

## What We Are Doing
- **Unity Client:** 
  - Utilizing Unity to build the game client, which interacts with the server using WebSocketSharp.
  - Implementing client-side logic to handle communication, player input, and synchronization with the server in real-time.
  
- **Visual Studio Console Server:** 
  - Developing the server as a console application in Visual Studio, responsible for managing game states, player positions, and interactions.
  - Using WebSocketSharp to manage WebSocket connections, handle incoming data from clients, and broadcast game updates.

- **WebSocketSharp Integration:**
  - Establishing and managing WebSocket connections between the Unity client and the Visual Studio console server.
  - Implementing server-side logic to handle real-time communication and synchronization of game states with connected clients.

- **Performance Profiling:**
  - Measuring the performance and scalability of this custom implementation.
  - Evaluating how many concurrent objects and players the network can handle efficiently.
  - Comparing this implementation with other multiplayer networking solutions like Photon Engine and Unity's Netcode for GameObjects to assess advantages and limitations.

## Conclusion
This custom client-server setup with WebSocketSharp provides a deep understanding of the intricacies involved in managing multiplayer game networking. It serves as a benchmark to compare with more abstracted solutions, offering insights into the trade-offs between control and convenience in multiplayer game development.
