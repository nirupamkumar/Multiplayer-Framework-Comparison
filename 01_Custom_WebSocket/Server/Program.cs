using System;
using System.Collections.Generic;
using System.IO;
using WebSocketSharp;
using WebSocketSharp.Server;
using ChatServer;

class Program
{
	static void Main(string[] args)
	{
		WebSocketServer server = new WebSocketServer(8080);
		server.AddWebSocketService<MUDServer>("/MUDServer");
		server.Start();
		MyLogger.WriteLog("Server started");
		while (true){}
	}

	public class MUDServer : WebSocketBehavior
	{
		static World world = new World();
		byte[] worldByte = SerializeWorld(world);
		static Player[] players = new Player[4];

		private static int rows = 18;
		private static int columns = 13;
		private static int[,] worldGrid = Util.Transform1DArrayTo2DArray(world.data, columns, rows);
		
		static Dictionary<string, Dictionary<string, string>> users = new Dictionary<string, Dictionary<string, string>>();

		private static Dictionary<string, Position> playerPositions = new Dictionary<string, Position>();

		protected override void OnOpen()
		{
			//send serialize world data to players
			Send(worldByte);
			if (players.Length > 0)
			{
				foreach (var player in players)
				{
					if (player == null) continue;
					var serializedPlayer = SerializePlayer(player);
					Send(serializedPlayer);
				}
			}
			base.OnOpen();
		}

		protected override void OnClose(CloseEventArgs e)
		{
			Dictionary<string, string> chatroom = null;
			if (!users.ContainsKey(Context.RequestUri.ToString()))
			{
				users[Context.RequestUri.ToString()] = new Dictionary<string, string>();
			}
			chatroom = users[Context.RequestUri.ToString()];
			
			if(chatroom.ContainsKey(ID))
			{
				MyLogger.WriteLog($"Player {chatroom[ID]} disconnected");
				chatroom.Remove(ID);
			}
			
		}

		protected override void OnMessage(MessageEventArgs e)
		{
			Dictionary<string, string> chatroom = null;
			if(!users.ContainsKey(Context.RequestUri.ToString()))
			{
				users[Context.RequestUri.ToString()] = new Dictionary<string, string>();
			}
			chatroom = users[Context.RequestUri.ToString()];
            if (e.Data != null)
            {
                if (e.Data.StartsWith("#name:"))
                {
                    chatroom[ID] = e.Data.Substring(6);
                    MyLogger.WriteLog($"Player {chatroom[ID]} connected");
                }
                else if (e.Data.StartsWith("#msg:"))
                {
                    var msg = e.Data.Substring(5) + "\n";
                    MyLogger.WriteLog($"Message sent: {msg}");
                    
                    foreach (var item in chatroom)
                    {
                        Sessions.SendTo(msg, item.Key);
                    }
                }
            }
			else if (e.RawData.Length > 0)
			{
				var sentPlayer = DeserializePlayer(e.RawData);
				if (players.Length > 0)
				{
					for (int i = 0; i < players.Length; i++)
					{
						if (players[i] == null)
						{
							players[i] = new Player(sentPlayer);
							playerPositions[sentPlayer.ID] = sentPlayer.position;
						}
						if (players[i].ID == sentPlayer.ID)
						{
							sentPlayer = CheckPlayerPosition(sentPlayer);
							players[i] = sentPlayer;
							break;
						}
						
						if (players[i].ID == null)
						{
							players[i] = sentPlayer;
							break;
						}
					}

					foreach (var item in chatroom)
					{
						var serializedPlayer = SerializePlayer(sentPlayer);
						Sessions.SendTo(serializedPlayer, item.Key);
					}
				}

			}
			base.OnMessage(e);
		}
		
		static Player CheckPlayerPosition(Player player)
		{
			var currentPosition = playerPositions[player.ID];
			
			var newPosX = currentPosition.x + player.direction.x;
			var newPosY = currentPosition.y + player.direction.y;

			var newPos = new Position(newPosX, newPosY);

			switch (worldGrid[(int)newPos.x, (int)newPos.y])
			{
				case (int)MapLegend.Tile:
				{
					MovePlayer(player, newPos);
					break;
				}
				case (int)MapLegend.Wall:
				{
					player.position = currentPosition;
					break;
				}
				case (int)MapLegend.Hole:
				{
					player.position = currentPosition;
					break;
				}
				case (int)MapLegend.Health:
				{
					MovePlayer(player, newPos);
					player.health += 50f;
					MyLogger.WriteLog($"Player {player.ID} picked up Health");
					break;
				}
				case (int)MapLegend.Attack:
				{
					MovePlayer(player, newPos);
					player.attack += 10f;
					MyLogger.WriteLog($"Player {player.ID} picked up Attack");
					break;
				}
				case (int)MapLegend.Speed:
				{
					MovePlayer(player, newPos);
					player.speed += 5f;
					MyLogger.WriteLog($"Player {player.ID} picked up Speed");
					break;
				}
			}

			return player;
		}

		static void MovePlayer(Player player, Position newPos)
		{
			MyLogger.WriteLog($"Player {player.ID} moved from {playerPositions[player.ID].x},{playerPositions[player.ID].y} to {newPos.x},{newPos.y}");
			player.position = newPos;
			playerPositions[player.ID] = newPos;
		}

        static byte[] SerializeWorld(World world)
        {
            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);

            bw.Write(world.data.Length);
            foreach (var item in world.data)
            {
                bw.Write(item);
            }

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

		static Player DeserializePlayer(byte[] data) {
			var ms = new MemoryStream(data);
			var br = new BinaryReader(ms);
			var player = new Player();

			player.ID = br.ReadString();
			var direction = new Direction
			{
				x = (int)br.ReadSingle(),
				y = (int)br.ReadSingle(),
				z = (int)br.ReadSingle()
			};
			player.direction = direction;
			
			var vector = new Position();
			vector.x = (int)br.ReadSingle();
			vector.y = (int)br.ReadSingle();
			vector.z = 0;
			
			player.health = br.ReadSingle();
			player.attack = br.ReadSingle();
			player.speed = br.ReadSingle();

			player.position = vector;

			ms.Close();
			br.Close();
			return player;
		}
		
		static byte[] SerializePlayer(Player player)
		{
			var ms = new MemoryStream();
			var bw = new BinaryWriter(ms);

			bw.Write(player.ID);
        
			bw.Write(player.direction.x);
			bw.Write(player.direction.y);
			bw.Write(player.direction.z);
        
			bw.Write(player.position.x);
			bw.Write(player.position.y);
			
			bw.Write(player.health);
			bw.Write(player.attack);
			bw.Write(player.speed);

			bw.Close();
			ms.Close();
			return ms.ToArray();
		}
	}

    class Player
	{
		public string ID;
		public Position position;
		public Direction direction;
		public float health;
		public float attack;
		public float speed;

		public Player(Player player)
		{
			ID = player.ID;
			position = player.position;
			direction = player.direction;
		}
		public Player()
		{
		}
	}
    
    struct Position
    {
	    public float x;
	    public float y;
	    public float z;

	    public Position(float x, float y, float z = 0)
	    {
		    this.x = x;
		    this.y = y;
		    this.z = z;
	    }
    }
    
    struct Direction
    {
	    public float x;
	    public float y;
	    public float z;
    }
    
    enum MapLegend
    {
	    Tile = 0,
	    Wall = 1,
	    Hole = 2,
	    Attack = 3,
	    Health = 4,
	    Speed = 5
    }
    
    [Serializable]
	class World
	{
		public int[] data = new int[] {1,1,1,1,1,1,1,1,1,1,1,1,1,
                                       1,0,0,0,0,0,0,0,0,0,0,0,1,
                                       1,0,0,0,2,2,2,0,3,0,2,0,1,
                                       1,0,0,0,0,0,0,0,0,0,0,0,1,
                                       1,0,0,2,2,0,0,0,2,0,0,0,1,
                                       1,0,0,0,0,0,0,0,0,0,0,0,1,
                                       1,1,1,1,1,1,0,1,1,1,1,1,1,
                                       1,0,0,0,0,0,0,0,4,0,0,0,1,
                                       1,0,0,0,2,2,2,0,0,0,2,0,1,
                                       1,0,0,0,0,0,0,0,0,0,0,0,1,
                                       1,0,0,2,2,0,2,0,2,0,0,0,1,
                                       1,0,0,0,0,0,2,2,2,2,2,2,1,
                                       1,0,1,1,1,1,1,1,1,1,1,1,1,
                                       1,0,0,0,2,0,0,0,0,0,0,0,1,
                                       1,0,0,0,0,0,0,0,0,0,2,0,1,
                                       1,2,2,0,0,0,0,0,0,2,0,5,1,
                                       1,2,2,2,2,0,2,0,2,0,0,0,1,
                                       1,1,1,1,1,1,1,1,1,1,1,1,1
		};
	}
}