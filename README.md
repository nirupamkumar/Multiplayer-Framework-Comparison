# Multiplayer-Framework-Comparison
 Graduation-Thesis

# Topic

Based on the market shares, it is evident that we are now in the era of multiplayer games. There is high demand for them, as we can see in online insights there are millions of players playing online games every day. Implementing multiplayer games can be challenging depending on the type of game and the network architecture one chooses. There are different approaches to creating multiplayer games, especially when it comes to implementing multiplayer servers.

The server needs to be robust to match the game objects, player positions, stats, matchmaking, and others. There are lot of different library implementations out there for creating multiplayer games, with three of them being dominant choices. Since every game requirement is different, the topic of this thesis is finding advantages and disadvantages between those implementations in the form of criteria for these three dominant choices.

This thesis aims to compare three approaches by using criteria like how many lines of code is needed to implement required functionalities, which approach has a steeper learning curve, how well they perform when it comes to standard multiplayer features, how many objects network can handle and others. To achieve this, this thesis will create three same MUD (multi-user dungeon) genre prototypes for each approach. First approach will be to write own client-server implementation. Second approach will be to use Photon library, and third approach will use Netcode for gameobjects multiplayer implementation.

For monitoring and results this thesis will use inbuilt stats, and multiplayer profiler tool to fill the criteria list.