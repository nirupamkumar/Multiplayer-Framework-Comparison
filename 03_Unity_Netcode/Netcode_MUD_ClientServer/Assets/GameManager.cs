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

    public int[] worldData = new int[] {1,1,1,1,1,1,1,1,1,1,1,1,1,
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
                                       1,1,1,1,1,1,1,1,1,1,1,1,1};

    public int rows = 18;
    public int columns = 13;
    public int[,] worldGrid;

    public event Action OnWorldInitialized;

    private void Start()
    {
        Debug.Log("GameManager Start method called.");
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Server started, initializing GameManager.");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            CreateWorld();
            Logger.Log("Server has started and is now listening for clients.");
        }
        else
        {
            Debug.LogWarning("OnServerStarted called but not running as server.");
        }
    }

    private void CreateWorld()
    {
        Debug.Log("GameManager: Initializing worldGrid...");

        // Assuming worldData is already set, transform it to a 2D array
        worldGrid = Transform1DArrayTo2DArray(worldData, columns, rows);

        if (worldGrid != null)
        {
            Debug.Log("GameManager: worldGrid successfully initialized.");
            OnWorldInitialized?.Invoke(); // Trigger the event
        }
        else
        {
            Debug.LogError("GameManager: worldGrid failed to initialize.");
        }

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector3 pos = new Vector3(i, j, 0);
                Debug.Log($"Instantiating object at position {pos} with type {worldGrid[i, j]}");

                GameObject prefabToInstantiate = null;

                switch (worldGrid[i, j])
                {
                    case (int)MapLegend.Tile:
                        prefabToInstantiate = pathPrefab;
                        break;
                    case (int)MapLegend.Wall:
                        prefabToInstantiate = wallPrefab;
                        break;
                    case (int)MapLegend.Hole:
                        prefabToInstantiate = holePrefab;
                        break;
                    case (int)MapLegend.Health:
                        prefabToInstantiate = healthPrefab;
                        break;
                    case (int)MapLegend.Attack:
                        prefabToInstantiate = attackPrefab;
                        break;
                    case (int)MapLegend.Speed:
                        prefabToInstantiate = speedPrefab;
                        break;
                    default:
                        Debug.LogWarning($"Unrecognized type {worldGrid[i, j]} at position {pos}");
                        break;
                }

                if (prefabToInstantiate != null)
                {
                    GameObject instantiatedObj = Instantiate(prefabToInstantiate, pos, Quaternion.identity);
                    var networkObject = instantiatedObj.GetComponent<NetworkObject>();
                    if (networkObject != null)
                    {
                        networkObject.Spawn(); // NetworkObject.Spawn() ensures the object is visible on clients
                    }
                    else
                    {
                        Debug.LogError("Prefab does not have a NetworkObject component.");
                    }
                }
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Logger.Log($"Client {clientId} connected to the server.");

        Vector3 randomPosition = GetRandomSpawnPosition();
        GameObject playerInstance = Instantiate(playerPrefab, randomPosition, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        Logger.Log($"Client {clientId} connected to the server at position {randomPosition}.");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Logger.Log($"Client {clientId} disconnected from the server.");
    }

    public Vector3 GetRandomSpawnPosition()
    {
        int randomX, randomY;
        do
        {
            randomX = Random.Range(0, columns);
            randomY = Random.Range(0, rows);
        } while (worldGrid[randomX, randomY] != (int)MapLegend.Tile); // Ensure spawning only on tiles

        return new Vector3(randomX, randomY, 0);
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
        if (inputArray == null || inputArray.Length == 0)
        {
            Debug.LogError("Input array for Transform1DArrayTo2DArray is null or empty.");
            return null;
        }

        if (columns <= 0 || rows <= 0)
        {
            Debug.LogError("Invalid dimensions for Transform1DArrayTo2DArray. Columns and rows must be greater than zero.");
            return null;
        }

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
                    Debug.LogError("Index out of bounds while converting 1D to 2D array.");
                    resultArray[i, j] = 0;
                }
            }
        }
        Debug.Log("Transform1DArrayTo2DArray completed successfully.");
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

