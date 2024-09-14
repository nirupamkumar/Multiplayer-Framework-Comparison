using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField nameField;
    public GameObject[] removeOnConnect;
    public GameObject[] activateOnConnect;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void JoinChatroom()
    {
        PhotonNetwork.NickName = nameField.text;
        PhotonNetwork.ConnectUsingSettings();

        foreach (var go in removeOnConnect)
        {
            go.SetActive(false);
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("MUDRoom", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        foreach (var go in activateOnConnect)
        {
            go.SetActive(true);
        }
    }
}
