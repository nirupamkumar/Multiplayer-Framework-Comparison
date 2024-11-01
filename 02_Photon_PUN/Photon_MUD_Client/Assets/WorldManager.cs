using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviourPunCallbacks
{
    public GameObject border;
    public GameObject path;
    public GameObject hole;
    public GameObject healthPickup;
    public GameObject attackPickup;
    public GameObject speedPickup;

    private int rows = 18;
    private int columns = 13;
    public static int[,] worldGrid;

    void Start()
    {
        StartCoroutine(WaitForSeedAndGenerateWorld());
    }

    IEnumerator WaitForSeedAndGenerateWorld()
    {
        while (!PhotonNetwork.InRoom || !PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("WorldSeed"))
        {
            yield return null;
        }

        int seed = (int)PhotonNetwork.CurrentRoom.CustomProperties["WorldSeed"];
        GenerateAndBuildWorld(seed);
    }

    void GenerateAndBuildWorld(int seed)
    {
        CustomLogger.Instance.Log("WorldManager: Generating world with seed " + seed);
        Debug.Log("WorldManager: Generating world with seed " + seed);
        Random.InitState(seed);
        worldGrid = GenerateWorldGrid();
        BuildWorld();
        CustomLogger.Instance.Log("WorldManager: World generated successfully.");
    }

    [PunRPC]
    void RPC_CreateWorld()
    {
        Debug.Log("RPC_CreateWorld is called");
        Debug.Log("Building the world...");
        worldGrid = GenerateWorldGrid();
        BuildWorld();
        Debug.Log("World built successfully.");
    }

    int[,] GenerateWorldGrid()
    {
        int[] data = new int[]
        {
            // World data array
            1,1,1,1,1,1,1,1,1,1,1,1,1,
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

        return Util.Transform1DArrayTo2DArray(data, columns, rows);
    }

    void BuildWorld()
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector3 pos = new Vector3(i, j, 0);
                switch (worldGrid[i, j])
                {
                    case (int)MapLegend.Tile:
                        Instantiate(path, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Wall:
                        Instantiate(border, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Hole:
                        Instantiate(hole, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Health:
                        Instantiate(healthPickup, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Attack:
                        Instantiate(attackPickup, pos, Quaternion.identity);
                        break;
                    case (int)MapLegend.Speed:
                        Instantiate(speedPickup, pos, Quaternion.identity);
                        break;
                }
            }
        }
    }

    public static MapLegend GetTileTypeAtPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);

        if (x >= 0 && x < worldGrid.GetLength(0) && y >= 0 && y < worldGrid.GetLength(1))
        {
            return (MapLegend)worldGrid[x, y];
        }
        else
        {
            return MapLegend.Wall; 
        }
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