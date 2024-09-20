using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public PlayerController localPlayer;

    public Text healthText;
    public Text attackText;
    public Text speedText;

    public override void OnJoinedRoom()
    {
        // Initiate world creation
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

        SpawnPlayer();

        Debug.Log("Joined room.");
    }

    void Update()
    {
        if (localPlayer)
        {
            UpdateStatsTexts();
        }
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
        // Collect all valid spawn positions
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

        // Choose a random position from the list
        if (validPositions.Count > 0)
        {
            int index = Random.Range(0, validPositions.Count);
            return validPositions[index];
        }
        else
        {
            // Fallback position if no valid positions found
            return Vector3.zero;
        }
    }
}
