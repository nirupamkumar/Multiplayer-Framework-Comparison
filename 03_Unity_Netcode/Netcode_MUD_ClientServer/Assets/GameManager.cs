using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public int[] worldData;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            CreateWorld();
        }
    }

    private void CreateWorld()
    {
        foreach (var data in worldData)
        {
            // Instantiate and network-spawn world objects based on data
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        var playerInstance = Instantiate(playerPrefab);
        playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}