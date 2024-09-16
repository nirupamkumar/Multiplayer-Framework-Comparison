using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField nameField;
    public Button connectButton;

    public GameObject connectUI;
    public GameObject chatUI;
    public GameObject statsUI;

    private static int playerCount = 1;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        connectButton.onClick.AddListener(JoinChatroom);

        // Ensure the initial UI states are correct
        connectUI.SetActive(true);
        chatUI.SetActive(false);
        statsUI.SetActive(false);
    }

    public void JoinChatroom()
    {
        string playerName = nameField.text;

        if (string.IsNullOrEmpty(playerName))
        {
            // Assign a default name if none is provided
            playerName = "Player" + playerCount;
            playerCount++;
        }

        PhotonNetwork.NickName = playerName;
        PhotonNetwork.ConnectUsingSettings();

        connectUI.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("MUDRoom", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        chatUI.SetActive(true);
        statsUI.SetActive(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // Reactivate the Connect UI if the connection fails
        connectUI.SetActive(true);

        chatUI.SetActive(false);
        statsUI.SetActive(false);
    }
}
