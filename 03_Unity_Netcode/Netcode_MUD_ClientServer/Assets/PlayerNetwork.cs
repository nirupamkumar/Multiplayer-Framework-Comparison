using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Globalization;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<float> Health = new NetworkVariable<float>();
    public NetworkVariable<float> Attack = new NetworkVariable<float>();
    public NetworkVariable<float> Speed = new NetworkVariable<float>();

    public GameObject playerMesh;
    private UIManager uiManager;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager not found in the scene.");
        }

        if (IsOwner)
        {
            Logger.LogPlayerAction(OwnerClientId, "Joined the game.");
        }
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

        if (Input.GetKeyDown(KeyCode.W))
            direction = new Vector3(0, 1, 0);
        if (Input.GetKeyDown(KeyCode.S))
            direction = new Vector3(0, -1, 0);
        if (Input.GetKeyDown(KeyCode.A))
            direction = new Vector3(-1, 0, 0);
        if (Input.GetKeyDown(KeyCode.D))
            direction = new Vector3(1, 0, 0);

        if (direction != Vector3.zero)
        {
            MovePlayerServerRpc(direction);
            Logger.LogPlayerAction(OwnerClientId, $"Moved to {Position.Value}");
        }
    }

    [ServerRpc]
    private void MovePlayerServerRpc(Vector3 direction)
    {
        Position.Value += direction;
        Logger.LogPlayerAction(OwnerClientId, $"Moved to new position: {Position.Value}");
    }

    [ServerRpc]
    public void PickupItemServerRpc(string itemType)
    {
        Logger.LogPlayerAction(OwnerClientId, $"Picked up {itemType}");
        // Update stats based on item type
        switch (itemType)
        {
            case "Health":
                Health.Value += 1; // Example value
                break;
            case "Attack":
                Attack.Value += 1; // Example value
                break;
            case "Speed":
                Speed.Value += 1; // Example value
                break;
        }
    }

    [ServerRpc]
    public void SendMessageServerRpc(string message)
    {
        Logger.LogChatMessage(OwnerClientId, message); // Log chat message
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

    private void OnDestroy()
    {
        if (IsOwner)
        {
            Logger.LogPlayerAction(OwnerClientId, "Left the game.");
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
        if (IsOwner)
        {
            uiManager.UpdateStatsUI(newHealth, Attack.Value, Speed.Value);
        }
    }

    private void OnAttackChanged(float oldAttack, float newAttack)
    {
        if (IsOwner)
        {
            uiManager.UpdateStatsUI(Health.Value, newAttack, Speed.Value);
        }
    }

    private void OnSpeedChanged(float oldSpeed, float newSpeed)
    {
        if (IsOwner)
        {
            uiManager.UpdateStatsUI(Health.Value, Attack.Value, newSpeed);
        }
    }
}
