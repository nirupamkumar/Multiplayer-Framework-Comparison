using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<float> Health = new NetworkVariable<float>(100.0f);
    public NetworkVariable<float> Attack = new NetworkVariable<float>(10.0f);
    public NetworkVariable<float> Speed = new NetworkVariable<float>(5.0f);

    public GameObject playerMesh;
    private UIManager uiManager;
    private GameManager gameManager;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        gameManager = FindObjectOfType<GameManager>();

        if (uiManager == null)
        {
            Debug.LogError("UIManager not found in the scene.");
        }

        if (gameManager != null)
        {
            gameManager.OnWorldInitialized += OnWorldInitialized;
            Debug.Log("GameManager successfully assigned.");

        }
        else
        {
            Debug.LogError("GameManager not found in the scene.");
        }

        StartCoroutine(WaitForWorldGridInitialization());

        if (IsOwner)
        {
            Logger.LogPlayerAction(OwnerClientId, "Joined the game.");
            UpdateUI();
        }
    }

    private IEnumerator WaitForWorldGridInitialization()
    {
        float timeout = 5.0f;
        float timer = 0f;

        while (gameManager == null || gameManager.worldGrid == null)
        {
            gameManager = FindObjectOfType<GameManager>();

            if (gameManager != null && gameManager.worldGrid != null)
            {
                Debug.Log("GameManager and worldGrid successfully initialized in PlayerNetwork.");
                break;
            }

            timer += Time.deltaTime;
            if (timer > timeout)
            {
                Debug.LogError("Timeout waiting for gameManager and worldGrid initialization.");
                yield break;
            }

            Debug.Log("Waiting for gameManager and worldGrid to initialize...");
            yield return null;
        }
    }

    private void OnWorldInitialized()
    {
        Debug.Log("GameManager has initialized worldGrid.");
    }

    private void Update()
    {
        if (IsOwner)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        Vector3 direction = Vector3.zero;
        Quaternion targetRotation = Quaternion.identity;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = new Vector3(0, 1, 0);
            targetRotation = Quaternion.Euler(0, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = new Vector3(0, -1, 0);
            targetRotation = Quaternion.Euler(0, 0, 180);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = new Vector3(-1, 0, 0);
            targetRotation = Quaternion.Euler(0, 0, 90);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = new Vector3(1, 0, 0);
            targetRotation = Quaternion.Euler(0, 0, -90);
        }

        if (direction != Vector3.zero)
        {
            AttemptMove(direction);
            RotatePlayer(targetRotation);
            Logger.LogPlayerAction(OwnerClientId, $"Moved to {Position.Value}");
        }
    }

    private void RotatePlayer(Quaternion targetRotation)
    {
        if (playerMesh != null)
        {
            playerMesh.transform.rotation = targetRotation;
            SyncRotationServerRpc(targetRotation);
        }
        else
        {
            Debug.LogError("playerMesh is null. Cannot rotate player.");
        }
    }

    [ServerRpc]
    private void SyncRotationServerRpc(Quaternion targetRotation)
    {
        SyncRotationClientRpc(targetRotation);
    }

    [ClientRpc]
    private void SyncRotationClientRpc(Quaternion targetRotation)
    {
        if (!IsOwner)
        {
            playerMesh.transform.rotation = targetRotation;
        }
    }

    private void AttemptMove(Vector3 direction)
    {
        if (gameManager == null || gameManager.worldGrid == null)
        {
            Debug.LogError("GameManager or worldGrid is null. Cannot move player.");
            return;
        }

        Vector3 targetPosition = Position.Value + direction;
        if (CanMoveTo(targetPosition))
        {
            MovePlayerServerRpc(direction);
        }
        else
        {
            Debug.LogError("Cannot move to the desired position. Movement blocked.");
        }
    }

    private bool CanMoveTo(Vector3 targetPosition)
    {
        if (gameManager == null || gameManager.worldGrid == null)
        {
            Debug.LogError("worldGrid is null in GameManager. Cannot check movement.");
            return false;
        }

        int x = Mathf.RoundToInt(targetPosition.x);
        int y = Mathf.RoundToInt(targetPosition.y);

        if (x < 0 || x >= gameManager.columns || y < 0 || y >= gameManager.rows)
        {
            Debug.LogError($"Invalid grid position: {x}, {y}. Out of bounds.");
            return false;
        }

        int tileType = gameManager.worldGrid[x, y];
        return tileType == (int)MapLegend.Tile || tileType == (int)MapLegend.Health ||
               tileType == (int)MapLegend.Attack || tileType == (int)MapLegend.Speed;
    }

    [ServerRpc]
    private void MovePlayerServerRpc(Vector3 direction)
    {
        Position.Value += direction;
        MovePlayerClientRpc(Position.Value);
    }

    [ClientRpc]
    private void MovePlayerClientRpc(Vector3 newPosition)
    {
        playerMesh.transform.position = newPosition;
    }

    [ServerRpc]
    public void PickupItemServerRpc(string itemType)
    {
        Logger.LogPlayerAction(OwnerClientId, $"Picked up {itemType}");

        switch (itemType)
        {
            case "Health":
                Health.Value += 50f;
                break;
            case "Attack":
                Attack.Value += 10f;
                break;
            case "Speed":
                Speed.Value += 5f;
                break;
        }
    }

    [ServerRpc]
    public void SendMessageServerRpc(string message)
    {
        Logger.LogChatMessage(OwnerClientId, message);
        AppendMessageClientRpc($"{OwnerClientId}: {message}");
    }

    [ClientRpc]
    private void AppendMessageClientRpc(string message)
    {
        if (uiManager != null)
        {
            uiManager.AppendChatMessage(message);
        }
        else
        {
            Debug.LogError("UIManager is not set or has been destroyed.");
        }
    }

    private void UpdateUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateStatsUI(Health.Value, Attack.Value, Speed.Value);
        }
        else
        {
            Debug.LogError("UIManager is not set or has been destroyed.");
        }
    }

    private void OnDestroy()
    {
        if (IsOwner)
        {
            Logger.LogPlayerAction(OwnerClientId, "Left the game.");
        }

        if (gameManager != null)
        {
            gameManager.OnWorldInitialized -= OnWorldInitialized;
        }
    }

    private void OnEnable()
    {
        Position.OnValueChanged += OnPositionChanged;
        Health.OnValueChanged += OnHealthChanged;
        Attack.OnValueChanged += OnAttackChanged;
        Speed.OnValueChanged += OnSpeedChanged;
    }

    private void OnDisable()
    {
        Position.OnValueChanged -= OnPositionChanged;
        Health.OnValueChanged -= OnHealthChanged;
        Attack.OnValueChanged -= OnAttackChanged;
        Speed.OnValueChanged -= OnSpeedChanged;
    }

    private void OnPositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        if (playerMesh != null)
        {
            playerMesh.transform.position = newPosition;
        }
    }

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        if (IsOwner) UpdateUI();
    }

    private void OnAttackChanged(float oldAttack, float newAttack)
    {
        if (IsOwner) UpdateUI();
    }

    private void OnSpeedChanged(float oldSpeed, float newSpeed)
    {
        if (IsOwner) UpdateUI();
    }
}