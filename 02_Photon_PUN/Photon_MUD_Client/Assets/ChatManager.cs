using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviourPun
{
    public InputField chatInputField;
    public Button sendMessage;
    public Text chatDisplay;

    void Start()
    {
        sendMessage.interactable = false;

        chatInputField.onValueChanged.AddListener(OnChatInputChanged);
        sendMessage.onClick.AddListener(OnSendButtonClicked);
    }

    void OnChatInputChanged(string text)
    {
        sendMessage.interactable = !string.IsNullOrEmpty(text.Trim());
    }

    void OnSendButtonClicked()
    {
        string message = chatInputField.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            photonView.RPC("RPC_ReceiveChatMessage", RpcTarget.All, PhotonNetwork.NickName, message);
            chatInputField.text = "";
            chatInputField.ActivateInputField();
        }
    }

    [PunRPC]
    void RPC_ReceiveChatMessage(string senderName, string message)
    {
        chatDisplay.text += $"\n{senderName}: {message}";
    }
}
