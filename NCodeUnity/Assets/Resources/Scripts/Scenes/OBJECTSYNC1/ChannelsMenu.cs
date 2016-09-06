using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using NCode.Core.Client;
using NCode;
using System.Collections.Generic;

public class ChannelsMenu : MonoBehaviour
{
    public InputField idField;
    public GameObject ConnectedChannelParent;
    Dictionary<int, GameObject> ConnectedChannels = new Dictionary<int, GameObject>();

    void Start()
    {
        NClientManager.onJoinChannel += OnJoinChannel;
        NClientManager.onLeaveChannel += OnLeaveChannel;
        NClientManager.JoinChannel(1);
        NClientManager.CreateNewObject(1, 1, true, new Vector3(0,10,0), new Quaternion(0, 0, 0, 0));
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

    void OnJoinChannel(int ID, bool Successful)
    {
        if (ID > 0 && Successful)
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
        if (ConnectedChannels.ContainsKey(ID) && Successful)
        {
            DestroyImmediate(ConnectedChannels[ID]);
            ConnectedChannels.Remove(ID);
        }
    }
}
