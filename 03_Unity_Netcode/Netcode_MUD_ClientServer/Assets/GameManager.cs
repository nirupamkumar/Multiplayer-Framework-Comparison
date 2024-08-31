using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject pathPrefab;
    public GameObject wallPrefab;
    public GameObject holePrefab;
    public GameObject healthPrefab;
    public GameObject attackPrefab;
    public GameObject speedPrefab;

    public int[] worldData; // Predefined world data array
    private int rows = 18;
    private int columns = 13;
    private int[,] worldGrid;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            CreateWorld();
            Logger.Log("Server has started and is now listening for clients.");
        }
    }

    private void CreateWorld()
    {
        worldGrid = Transform1DArrayTo2DArray(worldData, columns, rows);

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector3 pos = new Vector3(i, j, 0);
                Debug.Log($"Instantiating object at position {pos} with type {worldGrid[i, j]}");

                switch (worldGrid[i, j])
                {
                    case (int)MapLegend.Tile:
                        Instantiate(pathPrefab, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Wall:
                        Instantiate(wallPrefab, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Hole:
                        Instantiate(holePrefab, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Health:
                        Instantiate(healthPrefab, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Attack:
                        Instantiate(attackPrefab, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Speed:
                        Instantiate(speedPrefab, pos, Quaternion.identity);
                        break;
                }
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Vector3 randomPosition = GetRandomSpawnPosition();
        Debug.Log($"Spawning player {clientId} at position {randomPosition}");
        var playerInstance = Instantiate(playerPrefab, randomPosition, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        Logger.Log($"Client {clientId} connected to the server at position {randomPosition}.");
    }


    private Vector3 GetRandomSpawnPosition()
    {
        // Example logic to get a random spawn position within the world grid
        // Make sure you adjust this logic based on your world dimensions
        int randomX = Random.Range(0, columns);
        int randomY = Random.Range(0, rows);

        // Adjust z-axis or y-axis based on your 2D/3D setup
        return new Vector3(randomX, randomY, 0);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Logger.Log($"Client {clientId} disconnected from the server.");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            Logger.Log("Server has stopped.");
        }
    }

    public static int[,] Transform1DArrayTo2DArray(int[] inputArray, int columns, int rows)
    {
        var resultArray = new int[columns, rows];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                int index = i * columns + j;
                if (index < inputArray.Length)
                {
                    resultArray[j, i] = inputArray[index];
                }
                else
                {
                    resultArray[i, j] = 0;
                }
            }
        }

        return resultArray;
    }
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