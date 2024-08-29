using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class UIManager : MonoBehaviour
{
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
        connectButton.onClick.AddListener(OnConnectButtonClicked);
        sendButton.onClick.AddListener(OnSendButtonClicked);

        // Disable chat and stats initially
        ToggleChatUI(false);
        ToggleStatsUI(false);
    }

    private void OnConnectButtonClicked()
    {
        // Attempt to start as client
        NetworkManager.Singleton.StartClient();
        ToggleChatUI(true); // Enable chat UI on connect
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

    private void ToggleChatUI(bool enable)
    {
        chatInputField.gameObject.SetActive(enable);
        sendButton.gameObject.SetActive(enable);
        chatText.gameObject.SetActive(enable);
    }

    private void ToggleStatsUI(bool enable)
    {
        healthText.gameObject.SetActive(enable);
        attackText.gameObject.SetActive(enable);
        speedText.gameObject.SetActive(enable);
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
