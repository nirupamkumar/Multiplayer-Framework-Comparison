using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.IO;
using Photon.Pun.Demo.PunBasics;

public class PhotonCloudClient : MonoBehaviourPunCallbacks
{
    public GameObject[] removeOnConnect; // UI elements to disable upon connection
    public GameObject[] activateOnConnect; // UI elements to enable upon connection
    public InputField nameField; // Input field for player name
    public InputField textField; // Input field for messages
    public GameManager gameManager; // Reference to the GameManager
    public Text textMessage; // UI Text to display chat messages

    private List<string> serverMessages = new List<string>(); // List to store incoming messages

    void Start()
    {
        ConnectToPhotonCloud();
    }

    void ConnectToPhotonCloud()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // Sync scenes automatically for all clients
        PhotonNetwork.NickName = nameField.text; // Set the player's nickname
        PhotonNetwork.ConnectUsingSettings(); // Connect to Photon Cloud using settings in PhotonServerSettings
        Debug.Log("Connecting to Photon Cloud...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Cloud Master Server");
        JoinChatroom("DefaultRoom"); // Join or create a default room
    }

    public void JoinChatroom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 }; // Define room options
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);

        // Disable UI elements that are not needed after connection
        foreach (var go in removeOnConnect)
        {
            go.SetActive(false);
        }

        // Enable UI elements needed after connection
        foreach (var go in activateOnConnect)
        {
            go.SetActive(true);
        }

        // Notify other players that a new player has joined
        photonView.RPC("NotifyJoin", RpcTarget.All, PhotonNetwork.NickName);
    }

    [PunRPC]
    public void NotifyJoin(string playerName)
    {
        serverMessages.Add(playerName + " joined the chatroom.\n");
    }

    public void SendText()
    {
        // Send a chat message to all players in the room
        photonView.RPC("ReceiveMessage", RpcTarget.All, PhotonNetwork.NickName, textField.text);
        textField.text = ""; // Clear the input field
        textField.DeactivateInputField(); // Deactivate input field
    }

    [PunRPC]
    public void ReceiveMessage(string playerName, string message)
    {
        serverMessages.Add(playerName + ": " + message + "\n"); // Add the received message to the message list
    }

    void Update()
    {
        // Display all server messages in the chat window
        foreach (var message in serverMessages)
        {
            textMessage.text += message;
        }

        serverMessages.Clear();
    }

    void OnApplicationQuit()
    {
        PhotonNetwork.Disconnect(); // Disconnect from Photon Cloud when the application quits
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("Disconnected from Photon Cloud for reason {0}", cause);
    }
}
