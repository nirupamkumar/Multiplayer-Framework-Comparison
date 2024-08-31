using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject connectPanel;  // Panel containing the name field and connect button
    public GameObject chatPanel;     // Panel containing chat input and chat display
    public GameObject statsPanel;    // Panel containing player stats like health, attack, and speed

    [Header("UI Elements")]
    public Button connectButton;
    public InputField nameField;
    public InputField chatInputField;
    public Text chatText;
    public Button sendButton;
    public Text healthText;
    public Text attackText;
    public Text speedText;

    private void Start()
    {
        // Assign button click events
        sendButton.onClick.AddListener(OnSendButtonClicked);

        // Disable chat and stats initially
        chatPanel.SetActive(false);
        statsPanel.SetActive(false);
    }

    public void HideUI()
    {
        connectPanel.SetActive(false);
        chatPanel.SetActive(false);
        statsPanel.SetActive(false);
    }

    public void TriggerConnect()
    {
        connectButton.onClick.Invoke();
        connectPanel.SetActive(false);
        chatPanel.SetActive(true);
        statsPanel.SetActive(true);
    }

    private void OnConnectButtonClicked()
    {
        // Attempt to start as client
        //NetworkManager.Singleton.StartClient();
        //ToggleChatUI(true); // Enable chat UI on connect
    }

    private void OnSendButtonClicked()
    {
        if (!string.IsNullOrEmpty(chatInputField.text))
        {
            SendMessageToServer(chatInputField.text);
            chatInputField.text = ""; // Clear input field after sending
        }
    }

    private void SendMessageToServer(string message)
    {
        // Use a client RPC to send messages from the client to the server
        if (NetworkManager.Singleton.IsClient)
        {
            var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if (localPlayer != null)
            {
                localPlayer.GetComponent<PlayerNetwork>().SendMessageServerRpc(message);
            }
        }
    }

    public void UpdateStatsUI(float health, float attack, float speed)
    {
        healthText.text = $"Health: {health}";
        attackText.text = $"Attack: {attack}";
        speedText.text = $"Speed: {speed}";
    }

    public void AppendChatMessage(string message)
    {
        chatText.text += message + "\n";
    }
}
