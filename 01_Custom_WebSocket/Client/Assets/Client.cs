using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using EventArgs = System.EventArgs;
using UnityEngine.UI;
using System.IO;

public class Client : MonoBehaviour
{
    public GameObject[] removeOnConnect;
    public GameObject[] activateOnConnect;
    public InputField nameField;
    public InputField textField;

    public GameManager gameManager;

    public Text textMessage;

    public WebSocket client;
    World worldToInstantiate = null;
    bool worldBuilt = false;
    List<MessageEventArgs> serverMessages = new List<MessageEventArgs>();

    public void JoinChatroom(string chatroomPath)
    {
        client = new WebSocket("ws://localhost:8080/" + chatroomPath);
        client.OnOpen += OnOpen;
        client.OnMessage += OnMessage;
        client.Connect();

        foreach (var go in removeOnConnect)
        {
            go.SetActive(false);
        }
        foreach (var go in activateOnConnect)
        {
            go.SetActive(true);
        }
    }

    public void SendText()
    {
        client.Send("#msg:" + nameField.text + ": " + textField.text);
        textField.text = "";
        textField.DeactivateInputField();
    }

    public void SendPlayerData(byte[] data)
    {
        client.Send(data);
    }

    public void OnOpen(object sender, EventArgs e)
    {
        client.Send("#name:" + nameField.text);
    }

    public void OnMessage(object sender, MessageEventArgs e)
    {
        lock (serverMessages)
        {
            serverMessages.Add(e);
        }
    }

    void Update()
    {
        lock (serverMessages)
        {
            foreach (var item in serverMessages)
            {
                if (item.IsBinary)
                {
                    if (!worldBuilt)
                    {
                        worldToInstantiate = DeserializeWorld(item.RawData);
                        gameManager.CreateWorld(worldToInstantiate);
                        worldToInstantiate = null;
                        worldBuilt = true;
                    }
                    else
                    {
                        gameManager.CreatePlayer(item.RawData);
                    }

                }
                else
                {
                    foreach (var msg in item.Data)
                    {
                        textMessage.text += msg;
                    };
                }
            }
            serverMessages.Clear();
        }
    }

    void OnApplicationQuit()
    {
        client.Close();
    }

    World DeserializeWorld(byte[] b)
    {
        var world = new World();
        var ms = new MemoryStream(b);
        var br = new BinaryReader(ms);
        var length = br.ReadInt32();
        world.data = new int[length];

        for (int i = 0; i < length; i++)
        {
            world.data[i] = br.ReadInt32();
        }
        br.Close();
        ms.Close();
        return world;
    }
}

public class World
{
    public int[] data;
}