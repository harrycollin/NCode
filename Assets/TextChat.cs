using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using NCode;
using System.IO;

public class TextChat : MonoBehaviour
{
    public InputField ChannelID;
    public InputField MainMessage;
    public GameObject MessageBox;
    public GameObject MessageBoxElement;

	
    void Awake()
    {
        NClientManager.SetPacketHandler(Packet.TextChat, PacketHandler);
    }

    void PacketHandler(Packet packet, BinaryReader reader)
    {
        if(packet == Packet.TextChat)
        {
            GameObject ele = (GameObject)Instantiate(MessageBoxElement);
            ele.transform.parent = MessageBox.transform;
            ele.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            ele.GetComponent<Text>().text = reader.ReadString();
            ele.GetComponent<Text>().color = Color.green;

        }
    }

    public void Send()
    {
        GameObject ele = (GameObject)Instantiate(MessageBoxElement);
        ele.transform.parent = MessageBox.transform;
        ele.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
        ele.GetComponent<Text>().text = MainMessage.text;
        ele.GetComponent<Text>().color = Color.blue;

        BinaryWriter writer = NClientManager.BeginSend(Packet.TextChat, true);
        writer.Write(MainMessage.text);
        NClientManager.EndSend();
    }

    public void JoinChannel()
    {
        NClientManager.JoinChannel(int.Parse(ChannelID.text));
    }

    public void LeaveChannel()
    {
        NClientManager.LeaveChannel(int.Parse(ChannelID.text));
    }

    public void ReceiveMessage(string message)
    {

    }
}

