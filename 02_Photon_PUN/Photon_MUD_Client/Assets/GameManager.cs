using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public GameObject playerPrefab;
    public PlayerController localPlayer;

    public Text healthText;
    public Text attackText;
    public Text speedText;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject worldManagerObject = GameObject.Find("WorldManager");
            if (worldManagerObject != null)
            {
                WorldManager worldManager = worldManagerObject.GetComponent<WorldManager>();
                worldManager.photonView.RPC("RPC_CreateWorld", RpcTarget.AllBuffered);
            }
            else
            {
                Debug.LogError("WorldManager object not found in the scene.");
            }
        }

        StartCoroutine(WaitForWorldAndSpawnPlayer());
        Debug.Log("Joined room.");
    }

    private IEnumerator WaitForWorldAndSpawnPlayer()
    {
        while (WorldManager.worldGrid == null)
        {
            Debug.Log("GameManager: Waiting for worldGrid to be initialized...");
            yield return null; 
        }

        Debug.Log("GameManager: worldGrid is initialized. Spawning player.");
        SpawnPlayer();
    }

    void Update()
    {
        if (localPlayer) UpdateStatsTexts();
    }

    private void UpdateStatsTexts()
    {
        healthText.text = $"Health: {localPlayer.health}";
        attackText.text = $"Attack: {localPlayer.attack}";
        speedText.text = $"Speed: {localPlayer.speed}";
    }

    void SpawnPlayer()
    {
        Vector3 spawnPosition = GetRandomValidSpawnPosition();
        Debug.Log("Spawning player at position: " + spawnPosition);
        GameObject playerObject = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
        localPlayer = playerObject.GetComponent<PlayerController>();
    }

    Vector3 GetRandomValidSpawnPosition()
    {
        var validPositions = new List<Vector3>();

        int columns = WorldManager.worldGrid.GetLength(0);
        int rows = WorldManager.worldGrid.GetLength(1);

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                MapLegend tileType = (MapLegend)WorldManager.worldGrid[x, y];
                if (tileType == MapLegend.Tile)
                {
                    validPositions.Add(new Vector3(x, y, 0));
                }
            }
        }

        if (validPositions.Count > 0)
        {
            int index = Random.Range(0, validPositions.Count);
            return validPositions[index];
        }
        else
        {
            return Vector3.zero;
        }
    }
}
