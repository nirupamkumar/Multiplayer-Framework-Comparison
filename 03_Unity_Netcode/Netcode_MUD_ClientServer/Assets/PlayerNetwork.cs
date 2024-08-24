using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Globalization;

public class PlayerNetwork : MonoBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<float> Health = new NetworkVariable<float>();
    public NetworkVariable<float> Attack = new NetworkVariable<float>();
    public NetworkVariable<float> Speed = new NetworkVariable<float>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            MovePlayerServerRpc(new Vector3(0, 1, 0));
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            MovePlayerServerRpc(new Vector3(0, -1, 0));
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            MovePlayerServerRpc(new Vector3(-1, 0, 0));
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            MovePlayerServerRpc(new Vector3(1, 0, 0));
        }
    }

    [ServerRpc]
    private void MovePlayerServerRpc(Vector3 direction)
    {
        Position.Value += direction;
    }

    private void OnPositionChanged(Vector3 oldPosition, Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    [ServerRpc]
    public void UpdatePlayerStatsServerRpc(float healthChange, float attackChange, float speedChange)
    {
        Health.Value += healthChange;
        Attack.Value += attackChange;
        Speed.Value += speedChange;
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

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        // Update UI or other logic here
    }

    private void OnAttackChanged(float oldAttack, float newAttack)
    {
        // Update UI or other logic here
    }

    private void OnSpeedChanged(float oldSpeed, float newSpeed)
    {
        // Update UI or other logic here
    }
}
