using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public GameObject border;
    public GameObject path;
    public GameObject hole;
    public GameObject health;
    public GameObject attack;
    public GameObject speed;
    public GameObject playerMesh;
    public Player player = new Player();
    public Client client;
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
        client = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Client>();
        worldGrid = new int[columns, rows];
    }

    private void UpdateStatsTexts()
    {
        healthText.text = $"Health: {player.health}";
        attackText.text = $"Attack: {player.attack}";
        speedText.text = $"Speed: {player.speed}";
    }

    private void Update()
    {
        if (!player.mesh || client.textField.isFocused) return;

        UpdateStatsTexts();

        var direction = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W))
        {
            direction = new Vector3(0, 1, 0);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            direction = new Vector3(0, -1, 0);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            direction = new Vector3(-1, 0, 0);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            direction = new Vector3(1, 0, 0);
        }

        if (direction == Vector3.zero) return;

        var newPos = player.mesh.transform.position + direction;

        switch (worldGrid[(int)newPos.x, (int)newPos.y])
        {
            case (int)MapLegend.Tile:
            {
                MovePlayer(player, direction);
                break;
            }
            case (int)MapLegend.Wall:
            {
                break;
            }
            case (int)MapLegend.Hole:
            {
                break;
            }
            case (int)MapLegend.Health:
            {
                MovePlayer(player, direction);
                player.health += 50f;
                break;
            }
            case (int)MapLegend.Attack:
            {
                MovePlayer(player, direction);
                player.attack += 10f;
                break;
            }
            case (int)MapLegend.Speed:
            {
                MovePlayer(player, direction);
                player.speed += 5f;
                break;
            }
        }
    }

    private void MovePlayer(Player player, Vector3 direction)
    {
        player.direction = direction;
        var targetPosition = player.mesh.transform.position + player.direction;

        if (targetPosition == player.mesh.transform.position || isPlayerMoving) return;
        
        StartCoroutine(LerpPosition(player.mesh.transform, targetPosition, 0.2f));
    }

    public void SendPlayerData(Player player)
    {
        var playerSerialized = SerializePlayer(player);
        client.SendPlayerData(playerSerialized);
    }

    public void CreateWorld(World world)
    {
        var suitableLocations = new List<Vector3>();
        worldGrid = Util.Transform1DArrayTo2DArray(world.data, columns, rows);

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

        player.ID = client.nameField.text;
        var randomLocationIndex = Random.Range(0, suitableLocations.Count);
        player.mesh = Instantiate(playerMesh, suitableLocations[randomLocationIndex], Quaternion.identity);
        player.direction = Vector3.zero;
        player.health = 100f;
        player.attack = 10f;
        player.speed = 5f;
        SendPlayerData(player);
        players[0] = player;
    }

    public void CreatePlayer(byte[] data)
    {
        var player = DeserializePlayer(data);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null) players[i] = new Player();
            if (players[i].ID == player.ID)
            {
                players[i].mesh.transform.position = player.mesh.transform.position;
                break;
            }
            else if (players[i].ID == null)
            {
                players[i] = player;
                players[i].mesh = Instantiate(playerMesh, player.mesh.transform.position, Quaternion.identity);
                break;
            }
        }
    }

    IEnumerator LerpPosition(Transform transform, Vector3 targetPosition, float duration)
    {
        isPlayerMoving = true;
        float time = 0;
        var startingScale = transform.localScale;
        var startPosition = transform.position;
        
        var newRotation = Quaternion.LookRotation(transform.position - targetPosition, Vector3.forward);
        newRotation.x = 0f;
        newRotation.y = 0f;

        transform.localScale *= 1.2f;
        while (time < duration)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, time / duration);
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);

            time += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
        transform.localScale = startingScale;
        isPlayerMoving = false;
        SendPlayerData(player);

    }

    public byte[] SerializePlayer(Player player)
    {
        var ms = new MemoryStream();
        var bw = new BinaryWriter(ms);

        bw.Write(player.ID);

        bw.Write(player.direction.x);
        bw.Write(player.direction.y);
        bw.Write(player.direction.z);

        bw.Write(player.mesh.transform.position.x);
        bw.Write(player.mesh.transform.position.y);
        
        bw.Write(player.health);
        bw.Write(player.attack);
        bw.Write(player.speed);

        bw.Close();
        ms.Close();
        return ms.ToArray();
    }

    public Player DeserializePlayer(byte[] data)
    {
        var ms = new MemoryStream(data);
        var br = new BinaryReader(ms);

        var player = new Player();
        player.ID = br.ReadString();

        var direction = Vector3.zero;
        direction.x = (int)br.ReadSingle();
        direction.y = (int)br.ReadSingle();
        direction.z = (int)br.ReadSingle();

        player.direction = direction;

        var vector = Vector3Int.zero;
        vector.x = (int)br.ReadSingle();
        vector.y = (int)br.ReadSingle();

        player.mesh = new GameObject();
        player.mesh.transform.position = (Vector3Int)vector;

        player.health = br.ReadSingle();
        player.attack = br.ReadSingle();
        player.speed = br.ReadSingle();

        ms.Close();
        br.Close();
        return player;
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