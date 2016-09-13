using UnityEngine;
using System.Collections;
using NCode.Core.Client;
using UnityEngine.UI;
using System.Net;
using System.Collections.Generic;
using NCode;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public GameObject ConnectedChannelParent;
    public GameObject statusBar;
    public InputField ipfield;
    public InputField portfield;
    public InputField idField;

    Dictionary<int, GameObject> ConnectedChannels = new Dictionary<int, GameObject>();

    // Use this for initialization
    void Start ()
    {
        // Creates an instance of the client manager. This is the entry point for anything network related.
        NClientManager.CreateInstance();
        NClientManager.onConnect += OnConnect;
        NClientManager.onDisconnect += OnDisconnect;
        NClientManager.onJoinChannel += OnJoinChannel;
        NClientManager.onLeaveChannel += OnLeaveChannel;
    }


    public void Connect()
    {
        if(ipfield != null && ipfield.text != null && portfield != null && portfield.text != null && !NClientManager.isTryingToConnect)
        {
            IPAddress ip;
            int port;
            if(IPAddress.TryParse(ipfield.text, out ip) && int.TryParse(portfield.text, out port))
            {
                NClientManager.Connect(ipfield.text, port);
            }
        }
    }

    public void LoadObjectSync1()
    {
        if (NClientManager.isConnected)
        {
            SceneManager.LoadScene(1);
        }

    }

    public void JoinChannel()
    {
        int channelID;
        if (int.TryParse(idField.text, out channelID) && NClientManager.isConnected)
        {
            NClientManager.JoinChannel(channelID);
        }
    }

    public void LeaveChannel()
    {
        int channelID;
        if (int.TryParse(idField.text, out channelID) && NClientManager.isConnected)
        {
            NClientManager.LeaveChannel(channelID);
        }
    }

    public void OnConnect()
    {
        statusBar.GetComponent<Image>().color = new Color(0, 255, 44, 112);
        statusBar.GetComponentInChildren<Text>().text = "Connected to: " + NClientManager.RemoteServerEndPoint.ToString();
    }

    void OnDisconnect()
    {
        statusBar.GetComponent<Image>().color = new Color(255, 0, 0, 112);
        statusBar.GetComponentInChildren<Text>().text = "Disconnected";
    }

    void OnJoinChannel(int ID, bool Successful)
    {
        if(ID > 0 && Successful)
        {
            GameObject newChannel = new GameObject("Channel" + ID);
            newChannel.AddComponent<Text>();
            newChannel.GetComponent<Text>().text = "CHANNEL:" + ID;
            newChannel.GetComponent<Text>().font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            newChannel.GetComponent<Text>().fontSize = 25;
            newChannel.transform.parent = ConnectedChannelParent.transform;
            ConnectedChannels.Add(ID, newChannel);
        }
    }

    void OnLeaveChannel(int ID, bool Successful)
    {
        Tools.Print("assssss");

        if (ConnectedChannels.ContainsKey(ID) && Successful)
        {
            Tools.Print("as");
            DestroyImmediate(ConnectedChannels[ID]);
            ConnectedChannels.Remove(ID);
        }
    }
}
