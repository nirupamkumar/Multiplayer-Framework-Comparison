using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameNetManager : MonoBehaviour
{
    public UIManager uiManager;

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Logger.Log("Host started.");
        uiManager.HideUI();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Logger.Log("Client started.");
        uiManager.TriggerConnect();
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        Logger.Log("Server started.");
        uiManager.HideUI();
    }

    private void OnGUI()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 30;
            buttonStyle.alignment = TextAnchor.MiddleCenter;

            GUILayoutOption buttonHeight = GUILayout.Height(50);
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));

            if (GUILayout.Button("Start Client", buttonStyle, buttonHeight))
                StartClient();
            if (GUILayout.Button("Start Server", buttonStyle, buttonHeight))
                StartServer();

            GUILayout.EndArea();
        }
    }
}
