using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject border;
    public GameObject path;
    public GameObject hole;
    public GameObject health;
    public GameObject attack;
    public GameObject speed;
    public GameObject playerPrefab;

    public PlayerController localPlayer;

    public Text healthText;
    public Text attackText;
    public Text speedText;

    private int rows = 18;
    private int columns = 13;
    public int[,] worldGrid;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CreateWorld();
        }
    }

    public override void OnJoinedRoom()
    {
        if (!localPlayer)
        {
            SpawnPlayer();
        }
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

    public void CreateWorld()
    {
        PhotonNetwork.InstantiateRoomObject("WorldManager", Vector3.zero, Quaternion.identity);
    }

    void SpawnPlayer()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject playerObject = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
        localPlayer = playerObject.GetComponent<PlayerController>();
    }

    Vector3 GetRandomSpawnPosition()
    {
        // Implement logic to get a random spawn position on the map
        // For now, return a default position
        return new Vector3(1, 1, 0);
    }
}
