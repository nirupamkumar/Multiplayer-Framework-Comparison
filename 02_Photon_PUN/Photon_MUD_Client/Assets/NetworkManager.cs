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

        connectUI.SetActive(true);
        chatUI.SetActive(false);
        statsUI.SetActive(false);
    }

    public void CreateRoom()
    {
        string roomName = "Room_" + Random.Range(1000, 9999);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;

        int worldSeed = Random.Range(int.MinValue, int.MaxValue);
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties.Add("WorldSeed", worldSeed);
        roomOptions.CustomRoomProperties = customProperties;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "WorldSeed" };

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinChatroom()
    {
        //Debug.Log("Joined room with seed: " + PhotonNetwork.CurrentRoom.CustomProperties["WorldSeed"]);
        string playerName = nameField.text;

        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player" + playerCount;
            playerCount++;
        }

        PhotonNetwork.NickName = playerName;
        PhotonNetwork.ConnectUsingSettings();

        connectUI.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        //PhotonNetwork.JoinOrCreateRoom("MUDRoom", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
        CreateOrJoinRoom();
    }

    void CreateOrJoinRoom()
    {
        string roomName = "MUDRoom";
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;

        //if (!PhotonNetwork.InLobby)
        //{
        //    PhotonNetwork.JoinLobby();
        //}

        int worldSeed = Random.Range(int.MinValue, int.MaxValue);
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties.Add("WorldSeed", worldSeed);
        roomOptions.CustomRoomProperties = customProperties;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "WorldSeed" };

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        chatUI.SetActive(true);
        statsUI.SetActive(true);

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("WorldSeed"))
        {
            int seed = (int)PhotonNetwork.CurrentRoom.CustomProperties["WorldSeed"];
            Debug.Log("Joined room with seed: " + seed);
        }
        else
        {
            Debug.LogWarning("WorldSeed not found in room properties.");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        connectUI.SetActive(true);

        chatUI.SetActive(false);
        statsUI.SetActive(false);
    }
}
