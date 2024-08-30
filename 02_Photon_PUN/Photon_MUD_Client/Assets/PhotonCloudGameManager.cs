using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class PhotonCloudGameManager : MonoBehaviourPunCallbacks
{
    public GameObject border;
    public GameObject path;
    public GameObject hole;
    public GameObject health;
    public GameObject attack;
    public GameObject speed;
    public GameObject playerMesh;
    public Player player = new Player();
    public PhotonCloudClient client; // Reference to the Photon Cloud client script
    public Player[] players = new Player[4];

    public int[,] worldGrid;

    public Text healthText;
    public Text attackText;
    public Text speedText;

    private int rows = 18;
    private int columns = 13;
    private bool isPlayerMoving;

    private void Start()
    {
        client = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PhotonCloudClient>();
        worldGrid = new int[columns, rows];
    }

    private void Update()
    {
        if (!player.mesh || client.textField.isFocused) return;

        UpdateStatsTexts();

        if (photonView.IsMine) // Ensure the local player controls the character
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        var direction = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W)) direction = new Vector3(0, 1, 0);
        if (Input.GetKeyDown(KeyCode.S)) direction = new Vector3(0, -1, 0);
        if (Input.GetKeyDown(KeyCode.A)) direction = new Vector3(-1, 0, 0);
        if (Input.GetKeyDown(KeyCode.D)) direction = new Vector3(1, 0, 0);

        if (direction != Vector3.zero)
        {
            photonView.RPC("MovePlayer", RpcTarget.AllBuffered, player.ID, direction);
        }
    }

    [PunRPC]
    public void MovePlayer(string playerID, Vector3 direction)
    {
        Player playerToMove = FindPlayerByID(playerID);
        if (playerToMove == null || isPlayerMoving) return;

        playerToMove.direction = direction;
        Vector3 targetPosition = playerToMove.mesh.transform.position + playerToMove.direction;

        if (targetPosition == playerToMove.mesh.transform.position) return;

        StartCoroutine(LerpPosition(playerToMove.mesh.transform, targetPosition, 0.2f));
    }

    private Player FindPlayerByID(string playerID)
    {
        foreach (Player p in players)
        {
            if (p != null && p.ID == playerID) return p;
        }

        return null;
    }

    public void CreateWorld(World world)
    {
        // World creation logic as before...
        // Instantiate objects and initialize world
        var suitableLocations = new List<Vector3>();
        worldGrid = Util.Transform1DArrayTo2DArray(world.data, columns, rows); // Transform the world data into a 2D grid

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                var pos = new Vector3(i, j, 0);
                switch (worldGrid[i, j])
                {
                    case (int)MapLegend.Tile:
                        Instantiate(path, pos, Quaternion.identity);
                        suitableLocations.Add(pos);
                        break;
                    case (int)MapLegend.Wall:
                        Instantiate(border, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Hole:
                        Instantiate(hole, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Health:
                        Instantiate(health, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Attack:
                        Instantiate(attack, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Speed:
                        Instantiate(speed, pos, Quaternion.identity);
                        break;
                }
            }
        }

        // Initialize local player properties
        player.ID = PhotonNetwork.NickName; // Use Photon NickName as player ID
        var randomLocationIndex = Random.Range(0, suitableLocations.Count);
        player.mesh = Instantiate(playerMesh, suitableLocations[randomLocationIndex], Quaternion.identity);
        player.direction = Vector3.zero;
        player.health = 100f;
        player.attack = 10f;
        player.speed = 5f;

        // Inform other players of the new player
        photonView.RPC("InitializePlayer", RpcTarget.AllBuffered, player.ID, player.mesh.transform.position);

        photonView.RPC("InitializePlayer", RpcTarget.AllBuffered, player.ID, new Vector3(0, 0, 0)); // Example start position
    }

    [PunRPC]
    public void InitializePlayer(string playerID, Vector3 startPosition)
    {
        Player newPlayer = new Player
        {
            ID = playerID,
            mesh = Instantiate(playerMesh, startPosition, Quaternion.identity),
            direction = Vector3.zero,
            health = 100f,
            attack = 10f,
            speed = 5f
        };

        players[0] = newPlayer;
    }

    private void UpdateStatsTexts()
    {
        healthText.text = $"Health: {player.health}";
        attackText.text = $"Attack: {player.attack}";
        speedText.text = $"Speed: {player.speed}";
    }

    IEnumerator LerpPosition(Transform transform, Vector3 targetPosition, float duration)
    {
        isPlayerMoving = true;
        float time = 0;
        var startPosition = transform.position;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isPlayerMoving = false;
    }

    public class Player
    {
        public string ID;
        public Vector3 direction;
        public GameObject mesh;
        public float health;
        public float attack;
        public float speed;
    }

    public class World
    {
        public int[] data;
    }

    public enum MapLegend
    {
        Tile = 0,
        Wall = 1,
        Hole = 2,
        Attack = 3,
        Health = 4,
        Speed = 5
    }
}